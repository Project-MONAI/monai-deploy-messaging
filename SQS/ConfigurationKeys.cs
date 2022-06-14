// SPDX-FileCopyrightText: © 2021-2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.Messaging.Configuration
{
    internal static class SQSConfigurationKeys
    {
        public static readonly string AccessKey = "accessKey";
        public static readonly string AccessToken = "accessToken";
        public static readonly string Region = "region";
        public static readonly string WorkflowRequestQueue = "workflowRequestQueue";
        public static readonly string ExportRequestQueue = "exportRequestQueue";
        public static readonly string BucketName = "bucketName";
        public static readonly string Envid = "environmentId";

        public static readonly string[] PublisherRequiredKeys = new[] { WorkflowRequestQueue, BucketName };
        public static readonly string[] SubscriberRequiredKeys = new[] { ExportRequestQueue, BucketName };
    }
}
