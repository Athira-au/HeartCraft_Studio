# HeartCraft Studio

**HeartCraft Studio** is an interactive 3D visualization and simulation platform designed for medical education, anatomical exploration, and surgical training. Built with Unity and C#, it enables users to import heart models, perform surgical simulations, annotate anatomical structures, and export modified models—all within an intuitive, risk-free environment.

---

## About This Project

HeartCraft Studio was developed as part of the academic project **ITD415 - Project Phase II** at the **Government Engineering College Barton Hill**, Thiruvananthapuram.

- **Project Initiator:** The project was given by **Sree Chitra Tirunal Institute for Medical Sciences & Technology (SCTIMST)** , Thiruvananthapuram, a premier institution for medical research and cardiovascular care in India.
- **Collaboration:** The development was carried out in close collaboration with medical experts from SCTIMST to ensure clinical relevance and usability.
- **Data Source:** Anatomical reference data and model validation were based on resources sourced from the **National Institutes of Health (NIH)** website, ensuring medical accuracy and reliability.
- **Objective:** To provide medical students, researchers, and surgeons with an easy-to-use, free platform for exploring 3D heart anatomy and rehearsing surgical procedures on patient-specific models.

---

## Project Team

| Role | Name |
|------|------|
| **Team Members** | Athira A U (TRV221T020) |
| | Baby Shana (TRV221T021) |
| | Sivasandra Santhosh (TRV221T062) |
| | Vrinda A (TRV221T067) |
| **Project Guide** | Dr. Visakh R |
| **Department** | Information Technology |
| **Institution** | Government Engineering College Barton Hill, Thiruvananthapuram |
| **Collaborator** | Sree Chitra Tirunal Institute for Medical Sciences & Technology |

---

## Key Features

### 1. Model Import & Export
- Import standard **STL** files for real-time 3D mesh reconstruction.
- Export edited models as **STL** files for further simulation, analysis, or 3D printing.

### 2. Intuitive Visualization & Navigation
- **Free Rotation:** Click and drag to orbit around the model from any angle.
- **Axis-Based Rotation:** Rotate precisely along the X, Y, or Z axes with dedicated controls.
- **Zoom:** Use the scroll wheel to inspect fine anatomical details.

### 3. Surgical Simulation (Mesh Editing)
- **Planar Cutting:** Perform clean, straight cuts using axis-aligned slicing planes (X, Y, Z).
- **Freeform Cutting:** Draw custom strokes directly on the model surface for complex, organic cuts.
- **Patching:** Seamlessly repair and reconstruct mesh surfaces after cutting operations.

### 4. Annotation & Labelling
- Use raycasting to click and label specific anatomical structures directly on the 3D model.
- Labels help in educational identification and surgical planning.

### 5. Undo & Recovery
- Stack-based undo/redo functionality allows users to experiment freely without permanent errors.
- Full geometry restoration ensures safe and iterative exploration.

---

## Quick Start Guide

1. **Extract** the downloaded folder from git to a folder of your choice.
2. Open it in unity
3. Build and run the application
4. **Import a Model:** If you want you can import any `.stl` file using `Import STL` option.
5. **Explore & Edit:** Use the toolbar to rotate, cut, annotate, and modify the model.
6. **Export:** Save your work via `Export STL` for further use.

---

## Technology Stack

| Component | Technology Used |
|-----------|-----------------|
| **Game Engine** | Unity 2022.3 LTS |
| **Rendering Pipeline** | Universal Render Pipeline (URP) |
| **Programming Language** | C# |
| **User Interface** | Unity UI with TextMeshPro |
| **File Handling** | Standalone File Browser for native dialogs |
| **3D Model Format** | STL (Stereolithography) |

---


*HeartCraft Studio – Bridging Technology and Medicine for Better Cardiac Care.*
