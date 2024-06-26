/*
 * Copyright 2021-2024 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Monai.Deploy.Messaging.Events
{
    public enum ExportStatus
    {
        Success = 0,
        Failure,
        PartialFailure,
        Unknown
    }

    public enum FileExportStatus
    {
        /// <summary>
        /// File exported successfully
        /// </summary>
        Success,

        /// <summary>
        /// Export failed due to configuration error
        /// </summary>
        ConfigurationError,

        /// <summary>
        /// File is unsupported
        /// </summary>
        UnsupportedDataType,

        /// <summary>
        /// Error with the export service
        /// </summary>
        ServiceError,

        /// <summary>
        /// Error downloading file from storage service
        /// </summary>
        DownloadError,

        /// <summary>
        /// Unknown error
        /// </summary>
        Unknown,
    }
}
