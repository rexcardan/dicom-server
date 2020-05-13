﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Health.Dicom.Core {
    using System;


    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class DicomCoreResource {

        private static global::System.Resources.ResourceManager resourceMan;

        private static global::System.Globalization.CultureInfo resourceCulture;

        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal DicomCoreResource() {
        }

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.Health.Dicom.Core.DicomCoreResource", typeof(DicomCoreResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }

        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The limit must be between 1 and {0}..
        /// </summary>
        internal static string ChangeFeedLimitOutOfRange {
            get {
                return ResourceManager.GetString("ChangeFeedLimitOutOfRange", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The offset cannot be a negative value..
        /// </summary>
        internal static string ChangeFeedOffsetCannotBeNegative {
            get {
                return ResourceManager.GetString("ChangeFeedOffsetCannotBeNegative", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The data store operation failed..
        /// </summary>
        internal static string DataStoreOperationFailed {
            get {
                return ResourceManager.GetString("DataStoreOperationFailed", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The values for StudyInstanceUID, SeriesInstanceUID, SOPInstanceUID must be unique..
        /// </summary>
        internal static string DuplicatedUidsNotAllowed {
            get {
                return ResourceManager.GetString("DuplicatedUidsNotAllowed", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Duplicate AttributeId &apos;{0}&apos;, Each attribute is allowed to be specified once..
        /// </summary>
        internal static string DuplicateQueryParam {
            get {
                return ResourceManager.GetString("DuplicateQueryParam", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The specified frame cannot be found..
        /// </summary>
        internal static string FrameNotFound {
            get {
                return ResourceManager.GetString("FrameNotFound", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. IncludeField has unknown attribute {0}..
        /// </summary>
        internal static string IncludeFieldUnknownAttribute {
            get {
                return ResourceManager.GetString("IncludeFieldUnknownAttribute", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The DICOM instance already exists..
        /// </summary>
        internal static string InstanceAlreadyExists {
            get {
                return ResourceManager.GetString("InstanceAlreadyExists", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The specified instance cannot be found..
        /// </summary>
        internal static string InstanceNotFound {
            get {
                return ResourceManager.GetString("InstanceNotFound", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Specified date range &apos;{0}&apos; is invalid.
        ///The first part date {1} should be lesser than or equal to the second part date {2}..
        /// </summary>
        internal static string InvalidDateRangeValue {
            get {
                return ResourceManager.GetString("InvalidDateRangeValue", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Specified Date value {0} is invalid for parameter {1}. Date should be valid and formatted as yyyyMMdd..
        /// </summary>
        internal static string InvalidDateValue {
            get {
                return ResourceManager.GetString("InvalidDateValue", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to DICOM Identifier &apos;{0}&apos; value &apos;{1}&apos; is invalid. Value length should not exceed the maximum length of 64 characters. Value should contain characters in &apos;0&apos;-&apos;9&apos; and &apos;.&apos;. Each component must start with non-zero number..
        /// </summary>
        internal static string InvalidDicomIdentifier {
            get {
                return ResourceManager.GetString("InvalidDicomIdentifier", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The DICOM instance is invalid..
        /// </summary>
        internal static string InvalidDicomInstance {
            get {
                return ResourceManager.GetString("InvalidDicomInstance", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The specified frames value is not valid. At least one frame must be present, and all requested frames must have value greater than 0..
        /// </summary>
        internal static string InvalidFramesValue {
            get {
                return ResourceManager.GetString("InvalidFramesValue", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Specified limit value {0} is not a valid integer..
        /// </summary>
        internal static string InvalidLimitValue {
            get {
                return ResourceManager.GetString("InvalidLimitValue", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Specified offset value {0} is not a valid integer..
        /// </summary>
        internal static string InvalidOffsetValue {
            get {
                return ResourceManager.GetString("InvalidOffsetValue", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The specified Transfer Syntax value is not valid..
        /// </summary>
        internal static string InvalidTransferSyntaxValue {
            get {
                return ResourceManager.GetString("InvalidTransferSyntaxValue", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Specified fuzzymatch value {0} is not a valid boolean.
        /// </summary>
        internal static string InvaludFuzzyMatchValue {
            get {
                return ResourceManager.GetString("InvaludFuzzyMatchValue", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The specified item cannot be found..
        /// </summary>
        internal static string ItemNotFound {
            get {
                return ResourceManager.GetString("ItemNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The StudyInstanceUid &apos;{0}&apos; in the payload does not match the specified StudyInstanceUid &apos;{1}&apos;..
        /// </summary>
        internal static string MismatchStudyInstanceUid {
            get {
                return ResourceManager.GetString("MismatchStudyInstanceUid", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The request body is missing..
        /// </summary>
        internal static string MissingRequestBody {
            get {
                return ResourceManager.GetString("MissingRequestBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The required tag &apos;{0}&apos; is missing..
        /// </summary>
        internal static string MissingRequiredTag {
            get {
                return ResourceManager.GetString("MissingRequiredTag", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. AttributeId {0} has empty string value that is not supported..
        /// </summary>
        internal static string QueryEmptyAttributeValue {
            get {
                return ResourceManager.GetString("QueryEmptyAttributeValue", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Querying is only supported at resource level Studies/Series/Instances..
        /// </summary>
        internal static string QueryInvalidResourceLevel {
            get {
                return ResourceManager.GetString("QueryInvalidResourceLevel", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query.  Specified limit value {0} is outside the allowed range of {1}..{2}..
        /// </summary>
        internal static string QueryResultCountMaxExceeded {
            get {
                return ResourceManager.GetString("QueryResultCountMaxExceeded", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The specified series cannot be found..
        /// </summary>
        internal static string SeriesInstanceNotFound {
            get {
                return ResourceManager.GetString("SeriesInstanceNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The server is currently unable to receive requests. Please retry your request. If the issue persists, please contact support..
        /// </summary>
        internal static string ServiceUnavailable {
            get {
                return ResourceManager.GetString("ServiceUnavailable", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The specified study cannot be found..
        /// </summary>
        internal static string StudyInstanceNotFound {
            get {
                return ResourceManager.GetString("StudyInstanceNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. Unknown query parameter {0}..
        /// </summary>
        internal static string UnkownQueryParameter {
            get {
                return ResourceManager.GetString("UnkownQueryParameter", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The specified content type &apos;{0}&apos; is not supported..
        /// </summary>
        internal static string UnsupportedContentType {
            get {
                return ResourceManager.GetString("UnsupportedContentType", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to Invalid QIDO-RS query. AttributeId {0} is not queryable. .
        /// </summary>
        internal static string UnsupportedSearchParameter {
            get {
                return ResourceManager.GetString("UnsupportedSearchParameter", resourceCulture);
            }
        }

        /// <summary>
        ///   Looks up a localized string similar to The specified transcoding is not supported..
        /// </summary>
        internal static string UnsupportedTranscoding {
            get {
                return ResourceManager.GetString("UnsupportedTranscoding", resourceCulture);
            }
        }
    }
}
