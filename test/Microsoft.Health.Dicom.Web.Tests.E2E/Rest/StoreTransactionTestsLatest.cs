// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FellowOakDicom;
using Microsoft.Health.Dicom.Client;
using Microsoft.Health.Dicom.Tests.Common;
using Microsoft.Health.Dicom.Web.Tests.E2E.Common;
using Xunit;

namespace Microsoft.Health.Dicom.Web.Tests.E2E.Rest;

public class StoreTransactionTestsLatest : StoreTransactionTests
{
    public StoreTransactionTestsLatest(HttpIntegrationTestFixture<Startup> fixture) : base(fixture)
    {
    }

    protected override IDicomWebClient GetClient(HttpIntegrationTestFixture<Startup> fixture)
    {
        return fixture.GetDicomWebClient(DicomApiVersions.Latest);
    }

    [Fact]
    public async Task GivenInstanceWithAnInvalidIndexableAttribute_WhenEnableDropInvalidDicomJsonMetadata_ThenInvalidDataDroppedAndValidDataWritten()
    {
        // setup
        DicomFile dicomFile = GenerateDicomFile();

        DicomDataset dicomDataset = new DicomDataset().NotValidated();

        dicomDataset.Add(DicomTag.StudyDate, "NotAValidStudyDate");
        dicomDataset.Add(DicomTag.PatientBirthDate, "20220315");

        dicomFile.Dataset.Add(dicomDataset);

        // run
        DicomWebResponse<DicomDataset> response = await _instancesManager.StoreAsync(dicomFile);

        // assertions
        using DicomWebResponse<DicomFile> retrievedInstance = await _client.RetrieveInstanceAsync(
            dicomFile.Dataset.GetString(DicomTag.StudyInstanceUID),
            dicomFile.Dataset.GetString(DicomTag.SeriesInstanceUID),
            dicomFile.Dataset.GetString(DicomTag.SOPInstanceUID),
            dicomTransferSyntax: "*");

        DicomFile retrievedDicomFile = await retrievedInstance.GetValueAsync();

        // expect that valid attribute stored in dicom file
        Assert.Equal(
            dicomFile.Dataset.GetString(DicomTag.PatientBirthDate),
            retrievedDicomFile.Dataset.GetString(DicomTag.PatientBirthDate)
        );

        DicomDataset retrievedMetadata = await ResponseHelper.GetMetadata(_client, dicomFile);

        // expect valid data stored in metadata/JSON
        retrievedMetadata.GetString(DicomTag.PatientBirthDate);

        // valid searchable index attr was stored, so we can query for instance using the valid attr
        Assert.Single(await GetInstanceByAttribute(dicomFile, DicomTag.PatientBirthDate));

        // expect that metadata invalid date not present
        DicomDataException thrownException = Assert.Throws<DicomDataException>(
            () => retrievedMetadata.GetString(DicomTag.StudyDate));
        Assert.Equal("Tag: (0008,0020) not found in dataset", thrownException.Message);

        // attempting to query with invalid attr produces a BadRequest
        DicomWebException caughtException = await Assert.ThrowsAsync<DicomWebException>(() => GetInstanceByAttribute(dicomFile, DicomTag.StudyDate));

        Assert.Contains(
            "BadRequest: Invalid query: specified Date value 'NotAValidStudyDate' is invalid for attribute 'StudyDate'" +
            ". Date should be valid and formatted as yyyyMMdd.",
            caughtException.Message);

        // assert on response
        DicomDataset responseDataset = await response.GetValueAsync();
        DicomSequence refSopSequence = responseDataset.GetSequence(DicomTag.ReferencedSOPSequence);
        Assert.Single(refSopSequence);

        DicomDataset firstInstance = refSopSequence.Items[0];

        // expect a comment sequence present
        DicomSequence failedAttributesSequence = firstInstance.GetSequence(DicomTag.FailedAttributesSequence);
        Assert.Single(failedAttributesSequence);

        // expect comment sequence has single warning about single invalid attribute
        Assert.Equal(
            """DICOM100: (0008,0020) - Content "NotAValidStudyDate" does not validate VR DA: one of the date values does not match the pattern YYYYMMDD""",
            failedAttributesSequence.Items[0].GetString(DicomTag.ErrorComment)
        );
    }

    [Fact]
    public async Task GivenInstanceWithIndexableTagWithNullAsInvalidChar_WhenStoreInstance_ThenExpectDicom100ErrorAndAcceptedStatus()
    {
        // When null or other invalid chars encountered anywhere aside from with padding, we will drop that attribute and
        // respond with a warning
        string expectedValueWithNull = "X\0X";
        DicomFile dicomFile = new DicomFile(
            Samples.CreateRandomInstanceDataset(validateItems: false));

        DicomDataset dicomDataset = new DicomDataset().NotValidated();
        dicomDataset.Add(DicomTag.Modality, expectedValueWithNull);
        dicomFile.Dataset.Add(dicomDataset);

        DicomWebResponse<DicomDataset> response = await _instancesManager.StoreAsync(dicomFile);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        DicomDataset responseDataset = await response.GetValueAsync();

        DicomSequence refSopSequence = responseDataset.GetSequence(DicomTag.ReferencedSOPSequence);
        Assert.Single(refSopSequence);

        DicomDataset firstInstance = refSopSequence.Items[0];

        // expect a comment sequence to be empty
        DicomSequence failedAttributesSequence = firstInstance.GetSequence(DicomTag.FailedAttributesSequence);
        Assert.Contains(
            """does not validate VR CS: value contains invalid character""",
            failedAttributesSequence.Items[0].GetString(DicomTag.ErrorComment));
    }

    [Fact]
    public async Task GivenInstanceWithIndexableTagWithNullPadding_WhenStoreInstance_ThenOkAndNoWarning()
    {
        string expectedValueWithNull = "X\0";
        DicomFile dicomFile = new DicomFile(Samples.CreateRandomInstanceDataset(validateItems: false));

        DicomDataset dicomDataset = new DicomDataset().NotValidated();
        dicomDataset.Add(DicomTag.Modality, expectedValueWithNull);
        dicomFile.Dataset.Add(dicomDataset);
        DicomWebResponse<DicomDataset> response = await _instancesManager.StoreAsync(dicomFile);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // assert on response
        DicomDataset responseDataset = await response.GetValueAsync();
        DicomSequence refSopSequence = responseDataset.GetSequence(DicomTag.ReferencedSOPSequence);
        Assert.Single(refSopSequence);

        DicomDataset firstInstance = refSopSequence.Items[0];

        // expect a comment sequence to be empty
        DicomSequence failedAttributesSequence = firstInstance.GetSequence(DicomTag.FailedAttributesSequence);
        Assert.Empty(failedAttributesSequence);

        // expected dcm file has original value, will null padding
        using DicomWebResponse<DicomFile> retrievedInstance = await _client.RetrieveInstanceAsync(
            dicomFile.Dataset.GetString(DicomTag.StudyInstanceUID),
            dicomFile.Dataset.GetString(DicomTag.SeriesInstanceUID),
            dicomFile.Dataset.GetString(DicomTag.SOPInstanceUID),
            dicomTransferSyntax: "*");

        DicomFile retrievedDicomFile = await retrievedInstance.GetValueAsync();

        Assert.Equal(
            expectedValueWithNull,
            retrievedDicomFile.Dataset.GetString(DicomTag.Modality)
        );

        // expect stored metadata has original value, with null padding
        DicomDataset retrievedMetadata = await ResponseHelper.GetMetadata(_client, dicomFile);
        Assert.Equal(
            expectedValueWithNull,
            retrievedMetadata.GetString(DicomTag.Modality)
        );

        // expect that we can query for value with null padding as seen in data
        Assert.Single(await GetInstanceByAttribute(dicomFile, DicomTag.Modality));

        // and expect that we can query for value null padding when encoded as uri null
        using DicomWebAsyncEnumerableResponse<DicomDataset> qidoResponseWhenUrlEncoded = await _client.QueryInstancesAsync(
            queryString: "Modality=X%00");
        Assert.Single(await qidoResponseWhenUrlEncoded.ToArrayAsync());

        // and expect that we can query for value without padding at all
        using DicomWebAsyncEnumerableResponse<DicomDataset> qidoResponseWhenNoPadding = await _client.QueryInstancesAsync(
            queryString: "Modality=X");
        Assert.Single(await qidoResponseWhenNoPadding.ToArrayAsync());
    }

    [Fact]
    public async Task GivenInstanceWithStudyUIDValidWithNullPadding_WhenStoreInstanceWithPartialValidation_ThenExpectOkAndNoWarnings()
    {
        var validUID = TestUidGenerator.Generate();
        var validWithNullPadding = validUID + "\0";
        DicomFile dicomFile = new DicomFile(
            Samples.CreateRandomInstanceDataset(studyInstanceUid: validWithNullPadding, validateItems: false));

        try
        {
            DicomWebResponse<DicomDataset> response = await _client.StoreAsync(dicomFile, validUID);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // assert on response
            DicomDataset responseDataset = await response.GetValueAsync();
            DicomSequence refSopSequence = responseDataset.GetSequence(DicomTag.ReferencedSOPSequence);
            Assert.Single(refSopSequence);

            DicomDataset firstInstance = refSopSequence.Items[0];

            // expect a comment sequence to be empty
            DicomSequence failedAttributesSequence = firstInstance.GetSequence(DicomTag.FailedAttributesSequence);
            Assert.Empty(failedAttributesSequence);

            // ensure it can be queried with the valid uid
            DicomInstanceId id = DicomInstanceId.FromDicomFile(dicomFile);
            using DicomWebAsyncEnumerableResponse<DicomDataset> wadoResponse = await _client.RetrieveInstanceMetadataAsync(
                validUID,
                id.SeriesInstanceUid,
                id.SopInstanceUid);

            Assert.Equal(1, await wadoResponse.CountAsync());
        }
        finally
        {
            DicomInstanceId id = DicomInstanceId.FromDicomFile(dicomFile);
            await _client.DeleteInstanceAsync(
                validUID,
                id.SeriesInstanceUid,
                id.SopInstanceUid);
        }
    }

    [Fact]
    public async Task GivenInstanceWithPatientIdValidWithNullPadding_WhenStoreInstanceWithPartialValidation_ThenExpectOkAndNoWarnings()
    {
        var validPatientIdWithNullPadding = "123\0";
        DicomFile dicomFile1 = new DicomFile(
            Samples.CreateRandomInstanceDataset(patientId: validPatientIdWithNullPadding, validateItems: false));

        DicomWebResponse<DicomDataset> response = await _instancesManager.StoreAsync(dicomFile1);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // assert on response
        DicomDataset responseDataset = await response.GetValueAsync();
        DicomSequence refSopSequence = responseDataset.GetSequence(DicomTag.ReferencedSOPSequence);
        Assert.Single(refSopSequence);

        DicomDataset firstInstance = refSopSequence.Items[0];

        // expect a comment sequence to be empty
        DicomSequence failedAttributesSequence = firstInstance.GetSequence(DicomTag.FailedAttributesSequence);
        Assert.Empty(failedAttributesSequence);
    }

    [Fact]
    public async Task GivenInstanceWithPatientIdInvalidWithNullPadding_WhenStoreInstanceWithPartialValidation_ThenExpectConflict()
    {
        DicomFile dicomFile1 = new DicomFile(
            Samples.CreateRandomInstanceDataset(patientId: "\0123\0", validateItems: false));

        DicomWebException exception = await Assert.ThrowsAsync<DicomWebException>(() => _client.StoreAsync(dicomFile1));
        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);

        Assert.False(exception.ResponseDataset.TryGetSequence(DicomTag.ReferencedSOPSequence, out DicomSequence _));

        ValidationHelpers.ValidateFailedSopSequence(
            exception.ResponseDataset,
            ResponseHelper.ConvertToFailedSopSequenceEntry(dicomFile1.Dataset, ValidationHelpers.ValidationFailedFailureCode));
    }

    [Fact]
    public async Task GivenDatasetWithInvalidVrValue_WhenStoring_TheServerShouldReturnAccepted()
    {
        var studyInstanceUID = TestUidGenerator.Generate();

        DicomFile dicomFile1 = Samples.CreateRandomDicomFileWithInvalidVr(studyInstanceUID);

        DicomWebResponse<DicomDataset> response = await _instancesManager.StoreAsync(dicomFile1);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    }

    [Fact]
    [Trait("Category", "bvt")]
    public async Task GivenLargeSinglePartRequest_WhenStoring_ThenServerShouldReturnOk()
    {
        DicomFile dicomFile = Samples.CreateRandomDicomFileWithPixelData(
            rows: 42724,
            columns: 42724,
            dicomTransferSyntax: DicomTransferSyntax.ExplicitVRLittleEndian); // ~1.7 GB

        using DicomWebResponse<DicomDataset> stow = await _instancesManager.StoreAsync(dicomFile);
        Assert.Equal(HttpStatusCode.OK, stow.StatusCode);
    }

    [Fact]
    [Trait("Category", "bvt")]
    public async Task GivenLargeMultiPartRequest_WhenStoring_ThenServerShouldReturnOK()
    {
        // TODO Upload multiple large SOP instances at once
        string studyInstanceUid = TestUidGenerator.Generate();
        DicomFile[] files = Enumerable
            .Repeat(studyInstanceUid, 1)
            .Select(study => Samples.CreateRandomDicomFileWithPixelData(
                studyInstanceUid: study,
                rows: 42724,
                columns: 42724,
                dicomTransferSyntax: DicomTransferSyntax.ExplicitVRLittleEndian)) // ~1.7 GB
            .ToArray();

        using DicomWebResponse<DicomDataset> stow = await _instancesManager.StoreAsync(files);
        Assert.Equal(HttpStatusCode.OK, stow.StatusCode);
    }
}
