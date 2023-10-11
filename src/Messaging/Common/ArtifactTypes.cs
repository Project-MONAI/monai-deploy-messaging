namespace Monai.Deploy.Messaging.Common
{
    public static class ArtifactTypes
    {
        private static readonly Dictionary<ArtifactTypeEnum, string> ListOfModularity = new()
        {
            { ArtifactTypeEnum.Unset, "Unset" },
            { ArtifactTypeEnum.AR, "Autorefract" },
            { ArtifactTypeEnum.ASMT, "Content Assessment Results" },
            { ArtifactTypeEnum.AU, "Audio" },
            { ArtifactTypeEnum.BDUS, "Bone Densitometry (ultrasound)" },
            { ArtifactTypeEnum.BI, "Biomagnetic imaging" },
            { ArtifactTypeEnum.BMD, "Bone Densitometry (X-Ray)" },
            { ArtifactTypeEnum.CR, "Computed Radiography" },
            { ArtifactTypeEnum.CT, "Computed Tomography" },
            { ArtifactTypeEnum.DG, "Diaphanography" },
            { ArtifactTypeEnum.DOC, "Document" },
            { ArtifactTypeEnum.DX, "Digital Radiography" },
            { ArtifactTypeEnum.ECG, "Electrocardiography" },
            { ArtifactTypeEnum.EPS, "Cardiac Electrophysiology" },
            { ArtifactTypeEnum.ES, "Endoscopy" },
            { ArtifactTypeEnum.FID, "Fiducials" },
            { ArtifactTypeEnum.GM, "General Microscopy" },
            { ArtifactTypeEnum.HC, "Hard Copy" },
            { ArtifactTypeEnum.HD, "Hemodynamic Waveform" },
            { ArtifactTypeEnum.IO, "Intra-Oral Radiography" },
            { ArtifactTypeEnum.IOL, "Intraocular Lens Data" },
            { ArtifactTypeEnum.IVOCT, "Intravascular Optical Coherence Tomography" },
            { ArtifactTypeEnum.IVUS, "Intravascular Ultrasound" },
            { ArtifactTypeEnum.KER, "Keratometry" },
            { ArtifactTypeEnum.KO, "Key Object Selection" },
            { ArtifactTypeEnum.LEN, "Lensometry" },
            { ArtifactTypeEnum.LS, "Laser surface scan" },
            { ArtifactTypeEnum.MG, "Mammography" },
            { ArtifactTypeEnum.MR, "Magnetic Resonance" },
            { ArtifactTypeEnum.NM, "Nuclear Medicine" },
            { ArtifactTypeEnum.OAM, "Ophthalmic Axial Measurements" },
            { ArtifactTypeEnum.OCT, "Optical Coherence Tomography (non-Ophthalmic)" },
            { ArtifactTypeEnum.OP, "Ophthalmic Photography" },
            { ArtifactTypeEnum.OPM, "Ophthalmic Mapping" },
            { ArtifactTypeEnum.OPT, "Ophthalmic Tomography" },
            { ArtifactTypeEnum.OPV, "Ophthalmic Visual Field" },
            { ArtifactTypeEnum.OSS, "Optical Surface Scan" },
            { ArtifactTypeEnum.OT, "Other" },
            { ArtifactTypeEnum.PLAN, "Plan" },
            { ArtifactTypeEnum.PR, "Presentation State" },
            { ArtifactTypeEnum.PT, "Positron emission tomography (PET)" },
            { ArtifactTypeEnum.PX, "Panoramic X-Ray" },
            { ArtifactTypeEnum.REG, "Registration" },
            { ArtifactTypeEnum.RESP, "Respiratory Waveform" },
            { ArtifactTypeEnum.RF, "Radio Fluoroscopy" },
            { ArtifactTypeEnum.RG, "Radiographic imaging (conventional film/screen)" },
            { ArtifactTypeEnum.RTDOSE, "Radiotherapy Dose" },
            { ArtifactTypeEnum.RTIMAGE, "Radiotherapy Image" },
            { ArtifactTypeEnum.RTPLAN, "Radiotherapy Plan" },
            { ArtifactTypeEnum.RTRECORD, "RT Treatment Record" },
            { ArtifactTypeEnum.RTSTRUCT, "Radiotherapy Structure Set" },
            { ArtifactTypeEnum.RWV, "Real World Value Map" },
            { ArtifactTypeEnum.SEG, "Segmentation" },
            { ArtifactTypeEnum.SM, "Slide Microscopy" },
            { ArtifactTypeEnum.SMR, "Stereometric Relationship" },
            { ArtifactTypeEnum.SR, "SR Document" },
            { ArtifactTypeEnum.SRF, "Subjective Refraction" },
            { ArtifactTypeEnum.STAIN, "Automated Slide Stainer" },
            { ArtifactTypeEnum.TG, "Thermography" },
            { ArtifactTypeEnum.US, "Ultrasound" },
            { ArtifactTypeEnum.VA, "Visual Acuity" },
            { ArtifactTypeEnum.XA, "X-Ray Angiography" },
            { ArtifactTypeEnum.XC, "External-camera Photography" },
        };

        public static bool Validate(string artifactType)
        {
            return ListOfModularity.Any(x =>
                Enum.TryParse<ArtifactTypeEnum>(artifactType, out var artifact)
                && x.Key == artifact);
        }
    }
}
