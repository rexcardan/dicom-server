// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EnsureThat;
using FellowOakDicom;
using Microsoft.Health.Dicom.Client;
using Microsoft.Health.Dicom.Client.Models;
using Microsoft.Health.Dicom.Core.Extensions;
using Microsoft.Health.Dicom.Tests.Common;
using Microsoft.Health.Dicom.Web.Tests.E2E.Common;
using Microsoft.Health.Operations;
using Xunit;
using FunctionsStartup = Microsoft.Health.Dicom.Functions.App.Startup;
using WebStartup = Microsoft.Health.Dicom.Web.Startup;

namespace Microsoft.Health.Dicom.Web.Tests.E2E.Rest;

[Trait("Category", "bvt")]
[Collection("Update Collection")]
public class UpdateInstanceTests : IClassFixture<WebJobsIntegrationTestFixture<WebStartup, FunctionsStartup>>, IAsyncLifetime
{
    private readonly IDicomWebClient _client;
    private readonly DicomTagsManager _tagManager;
    private readonly DicomInstancesManager _instancesManager;

    public UpdateInstanceTests(WebJobsIntegrationTestFixture<WebStartup, FunctionsStartup> fixture)
    {
        EnsureArg.IsNotNull(fixture, nameof(fixture));
        _client = fixture.GetDicomWebClient();
        _tagManager = new DicomTagsManager(_client);
        _instancesManager = new DicomInstancesManager(_client);
    }

    [Fact]
    public async Task WhenUpdatingDicomMetadataForASingleStudy_ThenItShouldUpdateCorrectly()
    {
        string studyInstanceUid = TestUidGenerator.Generate();

        DicomFile dicomFile1 = Samples.CreateRandomDicomFile(studyInstanceUid);
        DicomFile dicomFile2 = Samples.CreateRandomDicomFile(studyInstanceUid);
        DicomFile dicomFile3 = Samples.CreateRandomDicomFile(studyInstanceUid);

        // Upload files
        Assert.True((await _instancesManager.StoreStudyAsync(new[] { dicomFile1, dicomFile2, dicomFile3 })).IsSuccessStatusCode);

        // Update study
        await UpdateStudyAsync(studyInstanceUid, "New^PatientName");

        // Verify study
        await VerifyMetadata(studyInstanceUid, Enumerable.Repeat("New^PatientName", 3).ToArray());
    }

    [Fact]
    public async Task WhenUpdatingDicomMetadataForStudyWithMultipleInstances_ThenItShouldUpdateCorrectly()
    {
        string studyInstanceUid1 = TestUidGenerator.Generate();
        string studyInstanceUid2 = TestUidGenerator.Generate();

        DicomFile dicomFile1 = Samples.CreateRandomDicomFileWithPixelData(studyInstanceUid1, rows: 200, columns: 200, frames: 10, dicomTransferSyntax: DicomTransferSyntax.ExplicitVRLittleEndian);
        DicomFile dicomFile2 = Samples.CreateRandomDicomFile(studyInstanceUid1);
        DicomFile dicomFile3 = Samples.CreateRandomDicomFileWithPixelData(studyInstanceUid2);
        string originalPatientName1 = dicomFile1.Dataset.GetSingleValue<string>(DicomTag.PatientName);
        string originalPatientName2 = dicomFile2.Dataset.GetSingleValue<string>(DicomTag.PatientName);

        // Upload files
        Assert.True((await _instancesManager.StoreAsync(new[] { dicomFile1, dicomFile2, dicomFile3 })).IsSuccessStatusCode);

        // Update study
        await UpdateStudyAsync(studyInstanceUid1, "New^PatientName");

        // Verify study
        await VerifyMetadata(studyInstanceUid1, Enumerable.Repeat("New^PatientName", 2).ToArray());
        await VerifyRetrieveInstance(studyInstanceUid1, dicomFile1, "New^PatientName");

        // Update again to ensure DICOM file is not corrupted after update
        await UpdateStudyAsync(studyInstanceUid1, "New^PatientName1");

        // Verify again to ensure update is successful
        await VerifyRetrieveInstance(studyInstanceUid1, dicomFile1, "New^PatientName1", true);
        await VerifyRetrieveInstanceWithTranscoding(studyInstanceUid1, dicomFile1, "New^PatientName1", true);
        await VerifyMetadata(studyInstanceUid1, new string[] { originalPatientName1, originalPatientName2 }, true);
        await VerifyRetrieveFrame(studyInstanceUid1, dicomFile1);
    }

    [Fact]
    public async Task GivenInstanceUpdated_WhenDeleting_ThenItShouldDeleteBothOriginalAndNew()
    {
        string studyInstanceUid1 = TestUidGenerator.Generate();
        DicomFile dicomFile1 = Samples.CreateRandomDicomFileWithPixelData(studyInstanceUid1, rows: 200, columns: 200, frames: 10, dicomTransferSyntax: DicomTransferSyntax.ExplicitVRLittleEndian);

        // Upload original file
        Assert.True((await _instancesManager.StoreAsync(new[] { dicomFile1 })).IsSuccessStatusCode);

        // Update study
        await UpdateStudyAsync(studyInstanceUid1, "New^PatientName");

        // call delete service and verify both new and original blobs deleted
        await VerifyDeleteStudyAsync(studyInstanceUid1, dicomFile1, requestOriginalVersion: true);
    }

    [Fact]
    public async Task WhenUpdatingDicomMetadataWithExtendedQueryTagForASingleStudy_ThenItShouldUpdateCorrectly()
    {
        DicomTag ageTag = DicomTag.PatientAge;
        DicomTag patientSexTag = DicomTag.PatientSex;
        string tagValue = "053Y";

        // Try to delete these extended query tags.
        await _tagManager.DeleteExtendedQueryTagAsync(ageTag.GetPath());

        string studyInstanceUid = TestUidGenerator.Generate();

        DicomFile dicomFile1 = Samples.CreateRandomDicomFile(studyInstanceUid);
        dicomFile1.Dataset.Add(ageTag, tagValue);

        DicomFile dicomFile2 = Samples.CreateRandomDicomFile(studyInstanceUid);
        dicomFile2.Dataset.Add(ageTag, tagValue);

        DicomFile dicomFile3 = Samples.CreateRandomDicomFile(studyInstanceUid);
        dicomFile3.Dataset.Add(ageTag, tagValue);

        // Upload files
        Assert.True((await _instancesManager.StoreStudyAsync(new[] { dicomFile1, dicomFile2, dicomFile3 })).IsSuccessStatusCode);

        // Add extended query tag
        Assert.Equal(
            OperationStatus.Succeeded,
            await _tagManager.AddTagsAsync(
                new AddExtendedQueryTagEntry { Path = ageTag.GetPath(), VR = ageTag.GetDefaultVR().Code, Level = QueryTagLevel.Study },
                new AddExtendedQueryTagEntry { Path = patientSexTag.GetPath(), VR = patientSexTag.GetDefaultVR().Code, Level = QueryTagLevel.Study }));

        // Update study
        await UpdateStudyAsync(studyInstanceUid, "New^PatientName", "054Y", "M");

        // Verify using QIDO
        DicomWebAsyncEnumerableResponse<DicomDataset> queryResponse = await _client.QueryInstancesAsync($"{ageTag.GetPath()}=054Y&{patientSexTag.GetPath()}=M");
        DicomDataset[] instances = await queryResponse.ToArrayAsync();
        Assert.Equal(3, instances.Length);
    }

    private async Task UpdateStudyAsync(string studyInstanceUid, string expectedPatientName, string age = null, string patientSex = null)
    {
        var datasetToUpdate = new DicomDataset();
        datasetToUpdate.AddOrUpdate(DicomTag.PatientName, expectedPatientName);

        if (!string.IsNullOrEmpty(age))
        {
            datasetToUpdate.AddOrUpdate(DicomTag.PatientAge, age);
        }

        if (!string.IsNullOrEmpty(patientSex))
        {
            datasetToUpdate.AddOrUpdate(DicomTag.PatientSex, patientSex);
        }

        Assert.Equal(OperationStatus.Succeeded, await _instancesManager.UpdateStudyAsync(new List<string> { studyInstanceUid }, datasetToUpdate));
    }

    private async Task VerifyRetrieveInstance(string studyInstanceUid, DicomFile dicomFile, string expectedPatientName, bool requestOriginalVersion = default)
    {
        using DicomWebResponse<DicomFile> instanceRetrieve = await _client.RetrieveInstanceAsync(
            studyInstanceUid,
            dicomFile.Dataset.GetSingleValue<string>(DicomTag.SeriesInstanceUID),
            dicomFile.Dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID),
            dicomTransferSyntax: "*");

        DicomFile retrievedDicomFile = await instanceRetrieve.GetValueAsync();

        Assert.Equal(expectedPatientName, retrievedDicomFile.Dataset.GetSingleValue<string>(DicomTag.PatientName));

        if (requestOriginalVersion)
        {
            using DicomWebResponse<DicomFile> instanceRetrieve1 = await _client.RetrieveInstanceAsync(
                studyInstanceUid,
                dicomFile.Dataset.GetSingleValue<string>(DicomTag.SeriesInstanceUID),
                dicomFile.Dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID),
                dicomTransferSyntax: "*",
                requestOriginalVersion: true);

            DicomFile retrievedDicomFile1 = await instanceRetrieve1.GetValueAsync();
            Assert.NotNull(retrievedDicomFile);
        }
    }

    private async Task VerifyMetadata(string studyInstanceUid, string[] expectedPatientNames, bool requestOriginalVersion = default)
    {
        // Verify study
        using DicomWebAsyncEnumerableResponse<DicomDataset> response = await _client.RetrieveStudyMetadataAsync(studyInstanceUid, requestOriginalVersion: requestOriginalVersion);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/dicom+json", response.ContentHeaders.ContentType.MediaType);

        DicomDataset[] datasets = await response.ToArrayAsync();

        Assert.Equal(expectedPatientNames.Length, datasets.Length);
        string[] actualPatientNames = datasets.Select(x => x.GetSingleValue<string>(DicomTag.PatientName)).ToArray();

        Assert.True(expectedPatientNames.All(x => actualPatientNames.Contains(x)));
    }

    private async Task VerifyRetrieveFrame(string studyInstanceUid, DicomFile dicomFile)
    {
        using DicomWebResponse<Stream> response = await _client.RetrieveSingleFrameAsync(
            studyInstanceUid,
            dicomFile.Dataset.GetSingleValue<string>(DicomTag.SeriesInstanceUID),
            dicomFile.Dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID),
            1);
        using Stream frameStream = await response.GetValueAsync();
        Assert.NotNull(frameStream);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task VerifyRetrieveInstanceWithTranscoding(string studyInstanceUid, DicomFile dicomFile, string expectedPatientName, bool requestOriginalVersion = default)
    {
        using DicomWebResponse<DicomFile> instanceRetrieve = await _client.RetrieveInstanceAsync(
            studyInstanceUid,
            dicomFile.Dataset.GetSingleValue<string>(DicomTag.SeriesInstanceUID),
            dicomFile.Dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID),
            dicomTransferSyntax: DicomTransferSyntax.JPEG2000Lossless.UID.UID);

        DicomFile retrievedDicomFile = await instanceRetrieve.GetValueAsync();

        Assert.Equal(expectedPatientName, retrievedDicomFile.Dataset.GetSingleValue<string>(DicomTag.PatientName));

        if (requestOriginalVersion)
        {
            using DicomWebResponse<DicomFile> instanceRetrieve1 = await _client.RetrieveInstanceAsync(
                studyInstanceUid,
                dicomFile.Dataset.GetSingleValue<string>(DicomTag.SeriesInstanceUID),
                dicomFile.Dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID),
                dicomTransferSyntax: DicomTransferSyntax.JPEG2000Lossless.UID.UID,
                requestOriginalVersion: true);
            DicomFile retrievedDicomFile1 = await instanceRetrieve1.GetValueAsync();
            Assert.NotNull(retrievedDicomFile1);
        }
    }

    private async Task VerifyDeleteStudyAsync(string studyInstanceUid, DicomFile dicomFile, bool requestOriginalVersion = default)
    {
        // When deleted an instance that has been updated, both new and original files must be deleted
        var seriesInstanceUID = dicomFile.Dataset.GetSingleValue<string>(DicomTag.SeriesInstanceUID);
        var sopInstanceUID = dicomFile.Dataset.GetSingleValue<string>(DicomTag.SOPInstanceUID);

        using (DicomWebResponse response = await _client.DeleteStudyAsync(studyInstanceUid))
        {
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        DicomWebException exception1 = await Assert.ThrowsAsync<DicomWebException>(
            () => _client.RetrieveInstanceAsync(studyInstanceUid, seriesInstanceUID, sopInstanceUID));
        Assert.Equal(HttpStatusCode.NotFound, exception1.StatusCode);

        DicomWebException exception2 = await Assert.ThrowsAsync<DicomWebException>(
            () => _client.RetrieveInstanceAsync(studyInstanceUid, seriesInstanceUID, sopInstanceUID, requestOriginalVersion: requestOriginalVersion));
        Assert.Equal(HttpStatusCode.NotFound, exception2.StatusCode);

        await Assert.ThrowsAsync<DicomWebException>(
            () => _client.RetrieveStudyMetadataAsync(studyInstanceUid, requestOriginalVersion: requestOriginalVersion));
        await Assert.ThrowsAsync<DicomWebException>(
            () => _client.RetrieveStudyMetadataAsync(studyInstanceUid));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _instancesManager.DisposeAsync();
    }
}
