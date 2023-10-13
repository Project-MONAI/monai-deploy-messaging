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
    public enum ArtifactType
    {
        Unset, //Unset
        Folder, // Folder
        AR, //Autorefraction
        ASMT, //Content Assessment Results
        AU, //Audio
        BDUS, //Bone Densitometry (ultrasound)
        BI, //Biomagnetic imaging
        BMD, //Bone Densitometry (X-Ray)
        CR, //Computed Radiography
        CT, //Computed Tomography
        DG, //Diaphanography
        DOC, //Document
        DX, //Digital Radiography
        ECG, //Electrocardiography
        EPS, //Cardiac Electrophysiology
        ES, //Endoscopy
        FID, //Fiducials
        GM, //General Microscopy
        HC, //Hard Copy
        HD, //Hemodynamic Waveform
        IO, //Intra-Oral Radiography
        IOL, //Intraocular Lens Data
        IVOCT, //Intravascular Optical Coherence Tomography
        IVUS, //Intravascular Ultrasound
        KER, //Keratometry
        KO, //Key Object Selection
        LEN, //Lensometry
        LS, //Laser surface scan
        MG, //Mammography
        MR, //Magnetic Resonance
        NM, //Nuclear Medicine
        OAM, //Ophthalmic Axial Measurements
        OCT, //Optical Coherence Tomography (non-Ophthalmic)
        OP, //Ophthalmic Photography
        OPM, //Ophthalmic Mapping
        OPT, //Ophthalmic Tomography
        OPV, //Ophthalmic Visual Field
        OSS, //Optical Surface Scan
        OT, //Other
        PLAN, //Plan
        PR, //Presentation State
        PT, //Positron emission tomography (PET)
        PX, //Panoramic X-Ray
        REG, //Registration
        RESP, //Respiratory Waveform
        RF, //Radio Fluoroscopy
        RG, //Radiographic imaging (conventional film/screen)
        RTDOSE, //Radiotherapy Dose
        RTIMAGE, //Radiotherapy Image
        RTPLAN, //Radiotherapy Plan
        RTRECORD, //RT Treatment Record
        RTSTRUCT, //Radiotherapy Structure Set
        RWV, //Real World Value Map
        SEG, //Segmentation
        SM, //Slide Microscopy
        SMR, //Stereometric Relationship
        SR, //SR Document
        SRF, //Subjective Refraction
        STAIN, //Automated Slide Stainer
        TG, //Thermography
        US, //Ultrasound
        VA, //Visual Acuity
        XA, //X-Ray Angiography
        XC, //External-camera Photography
    };
}
