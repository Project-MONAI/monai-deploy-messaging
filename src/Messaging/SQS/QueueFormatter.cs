using System.Text.RegularExpressions;

namespace Monai.Deploy.Messaging.SQS
{
    internal static class QueueFormatter
    {
        /// <summary>
        /// Returns an aggregate of the the environmentId, queueBasename nd topic as the name of the queue defined in SQS.
        /// The returned string is made compliant to SQS naming convention : It will replace non alphanumeric and other characters than "_" and "-", by an hyphen
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="queuebasename"></param>
        /// <param name="topic"></param>
        /// <returns>string</returns>
        public static string FormatQueueName(string environmentId, string? queuebasename, string topic)
        {

            string queue = $"{queuebasename}_{topic}";

            if (!string.IsNullOrEmpty(environmentId))
                queue = $"{environmentId}_{queue}";
            queue = Regex.Replace(queue, "[^a-zA-Z0-9_]", "-");
            if (queue.Length > 80)
                queue = queue.Substring(0, 80);
            return queue;
        }
    }
}
