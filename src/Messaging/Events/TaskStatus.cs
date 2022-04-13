// SPDX-FileCopyrightText: © 2022 MONAI Consortium
// SPDX-License-Identifier: Apache License 2.0

namespace Monai.Deploy.Messaging.Events
{
    public enum TaskStatus
    {
        Unknown,
        Created,
        Succeeded,
        Failed,
        Canceled,
    }
}
