/*
 * Copyright 2022-2023 MONAI Consortium
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Monai.Deploy.Messaging.Common
{
    public static class ArtifactTypes
    {
        private static readonly Dictionary<ArtifactType, string> ListOfModularity = new()
        {
            { ArtifactType.Unset, "Unset" },
            { ArtifactType.AR, "Autorefract" },
            { ArtifactType.ASMT, "Content Assessment Results" },
            { ArtifactType.AU, "Audio" },
            { ArtifactType.BDUS, "Bone Densitometry (ultrasound)" },
            { ArtifactType.BI, "Biomagnetic imaging" },
            { ArtifactType.BMD, "Bone Densitometry (X-Ray)" },
            { ArtifactType.CR, "Computed Radiography" },
            { ArtifactType.CT, "Computed Tomography" },
            { ArtifactType.DG, "Diaphanography" },
            { ArtifactType.DOC, "Document" },
            { ArtifactType.DX, "Digital Radiography" },
            { ArtifactType.ECG, "Electrocardiography" },
            { ArtifactType.EPS, "Cardiac Electrophysiology" },
            { ArtifactType.ES, "Endoscopy" },
            { ArtifactType.FID, "Fiducials" },
            { ArtifactType.GM, "General Microscopy" },
            { ArtifactType.HC, "Hard Copy" },
            { ArtifactType.HD, "Hemodynamic Waveform" },
            { ArtifactType.IO, "Intra-Oral Radiography" },
            { ArtifactType.IOL, "Intraocular Lens Data" },
            { ArtifactType.IVOCT, "Intravascular Optical Coherence Tomography" },
            { ArtifactType.IVUS, "Intravascular Ultrasound" },
            { ArtifactType.KER, "Keratometry" },
            { ArtifactType.KO, "Key Object Selection" },
            { ArtifactType.LEN, "Lensometry" },
            { ArtifactType.LS, "Laser surface scan" },
            { ArtifactType.MG, "Mammography" },
            { ArtifactType.MR, "Magnetic Resonance" },
            { ArtifactType.NM, "Nuclear Medicine" },
            { ArtifactType.OAM, "Ophthalmic Axial Measurements" },
            { ArtifactType.OCT, "Optical Coherence Tomography (non-Ophthalmic)" },
            { ArtifactType.OP, "Ophthalmic Photography" },
            { ArtifactType.OPM, "Ophthalmic Mapping" },
            { ArtifactType.OPT, "Ophthalmic Tomography" },
            { ArtifactType.OPV, "Ophthalmic Visual Field" },
            { ArtifactType.OSS, "Optical Surface Scan" },
            { ArtifactType.OT, "Other" },
            { ArtifactType.PLAN, "Plan" },
            { ArtifactType.PR, "Presentation State" },
            { ArtifactType.PT, "Positron emission tomography (PET)" },
            { ArtifactType.PX, "Panoramic X-Ray" },
            { ArtifactType.REG, "Registration" },
            { ArtifactType.RESP, "Respiratory Waveform" },
            { ArtifactType.RF, "Radio Fluoroscopy" },
            { ArtifactType.RG, "Radiographic imaging (conventional film/screen)" },
            { ArtifactType.RTDOSE, "Radiotherapy Dose" },
            { ArtifactType.RTIMAGE, "Radiotherapy Image" },
            { ArtifactType.RTPLAN, "Radiotherapy Plan" },
            { ArtifactType.RTRECORD, "RT Treatment Record" },
            { ArtifactType.RTSTRUCT, "Radiotherapy Structure Set" },
            { ArtifactType.RWV, "Real World Value Map" },
            { ArtifactType.SEG, "Segmentation" },
            { ArtifactType.SM, "Slide Microscopy" },
            { ArtifactType.SMR, "Stereometric Relationship" },
            { ArtifactType.SR, "SR Document" },
            { ArtifactType.SRF, "Subjective Refraction" },
            { ArtifactType.STAIN, "Automated Slide Stainer" },
            { ArtifactType.TG, "Thermography" },
            { ArtifactType.US, "Ultrasound" },
            { ArtifactType.VA, "Visual Acuity" },
            { ArtifactType.XA, "X-Ray Angiography" },
            { ArtifactType.XC, "External-camera Photography" },
        };

        public static bool Validate(string artifactType)
        {
            return ListOfModularity.Any(x =>
                Enum.TryParse<ArtifactType>(artifactType, out var artifact)
                && x.Key == artifact);
        }
    }
}
