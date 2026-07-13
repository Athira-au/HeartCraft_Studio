using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CutRuntimeController : MonoBehaviour
{
    public enum PlanarTool
    {
        None,
        X,
        Y,
        Z
    }

    private const string CuttingPlaneName = "RuntimeCutPlane";
    private const float PlaneSurfaceOffset = 0.5f;

    private Transform modelHolder;
    private Camera mainCamera;
    private GameObject cuttingPlaneObject;
    [SerializeField] private Material cutPlaneMaterial;
    private static Material runtimeCutPlaneTemplate;

    private CutExecutor cutExecutor;
    private FreeCutProcessor freeCutProcessor;
    private FreeCutDrawer freeCutDrawer;
    private MeshUndoManager undoManager;
    private PatchManager patchManager;

    private Transform activeModel;
    private Mesh originalModelMesh;
    private Transform originalMeshSource;
    private Vector3 currentPlaneAxis = Vector3.right;
    private PlanarTool activePlanarTool = PlanarTool.None;
    private bool cutModeWasActive;
    private bool cutPlaneArmed;
    private bool hasHoverPoint;
    private Vector3 hoverWorldPoint;

    public PlanarTool ActivePlanarTool => activePlanarTool;

    void Awake()
    {
        NormalizeDefaultModelIfNeeded();
        EnsureRuntimeSetup();
        EnsureToolButtonHighlighter();
        SyncActiveModel();
        CaptureCurrentModelAsOriginal();
        SnapPlaneToModel();
        ExitCutMode();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        EnsureRuntimeSetup();
        EnsureToolButtonHighlighter();
        SyncActiveModel();
        SyncCutModeState();
        UpdatePlanarCutHover();
        HandlePlanarCutClick();
        HandleShortcuts();
    }

    public void EnterCutMode()
    {
        if (GetActiveModel() == null)
            return;

        InteractionMode.FreeCutActive = false;
        LabelMode.labelMode = false;
        ModelRotateZoom.rotationLocked = true;
        EditMode.currentMode = EditMode.Mode.Cut;
        SyncCutModeState();
    }

    public void ExitCutMode()
    {
        EditMode.currentMode = EditMode.Mode.None;
        ModelRotateZoom.rotationLocked = false;
        cutModeWasActive = false;
        cutPlaneArmed = false;
        hasHoverPoint = false;
        activePlanarTool = PlanarTool.None;

        if (cuttingPlaneObject != null)
            cuttingPlaneObject.SetActive(false);
    }

    private void SyncCutModeState()
    {
        bool cutModeActive = EditMode.currentMode == EditMode.Mode.Cut;

        if (!cutModeActive)
        {
            cutPlaneArmed = false;
            hasHoverPoint = false;
            activePlanarTool = PlanarTool.None;
        }

        if (cutModeActive && !cutModeWasActive && cutPlaneArmed)
            SnapPlaneToModel();

        if (cuttingPlaneObject != null)
            cuttingPlaneObject.SetActive(cutModeActive && cutPlaneArmed && hasHoverPoint);

        cutModeWasActive = cutModeActive;
    }

    public void ApplyCut()
    {
        if (cutExecutor == null || !hasHoverPoint)
            return;

        MovePlaneToPoint(hoverWorldPoint);
        cutExecutor.ApplyCut();
        ExitCutMode();
    }

    private void UpdatePlanarCutHover()
    {
        if (EditMode.currentMode != EditMode.Mode.Cut || !cutPlaneArmed)
            return;

        if (InteractionMode.FreeCutActive || mainCamera == null)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            hasHoverPoint = false;
            if (cuttingPlaneObject != null)
                cuttingPlaneObject.SetActive(false);
            return;
        }

        if (TryGetHeartHit(out RaycastHit hit))
        {
            hasHoverPoint = true;
            hoverWorldPoint = hit.point;
            MovePlaneToPoint(hoverWorldPoint);

            if (cuttingPlaneObject != null)
                cuttingPlaneObject.SetActive(true);
        }
        else
        {
            hasHoverPoint = false;
            if (cuttingPlaneObject != null)
                cuttingPlaneObject.SetActive(false);
        }
    }

    private void HandlePlanarCutClick()
    {
        if (EditMode.currentMode != EditMode.Mode.Cut || !cutPlaneArmed)
            return;

        if (!hasHoverPoint)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        ApplyCut();
    }

    public void EnableFreeCut()
    {
        if (InteractionMode.FreeCutActive)
        {
            InteractionMode.FreeCutActive = false;
            ModelRotateZoom.rotationLocked = false;
            return;
        }

        StartCoroutine(EnableFreeCutDelayed());
    }

    IEnumerator EnableFreeCutDelayed()
    {
        yield return null;

        EnsureRuntimeSetup();

        if (freeCutDrawer == null)
            freeCutDrawer = FindFirstObjectByType<FreeCutDrawer>();

        if (freeCutProcessor == null)
            freeCutProcessor = FindFirstObjectByType<FreeCutProcessor>();

        if (freeCutDrawer == null || freeCutProcessor == null)
        {
            Debug.LogWarning("FreeCut components missing.");
            yield break;
        }

        EditMode.currentMode = EditMode.Mode.None;
        activePlanarTool = PlanarTool.None;
        cutPlaneArmed = false;
        hasHoverPoint = false;
        LabelMode.labelMode = false;

        if (cuttingPlaneObject != null)
            cuttingPlaneObject.SetActive(false);

        ModelRotateZoom.rotationLocked = true;
        InteractionMode.FreeCutActive = true;
        freeCutProcessor.allowCut = true;
        freeCutDrawer.EnableFreeCut();
    }

    public void SetCutPlaneX()
    {
        TogglePlanarCut(PlanarTool.X, Vector3.right);
    }

    public void SetCutPlaneY()
    {
        TogglePlanarCut(PlanarTool.Y, Vector3.up);
    }

    public void SetCutPlaneZ()
    {
        TogglePlanarCut(PlanarTool.Z, Vector3.forward);
    }

    public void ApplyCutX()
    {
        SetCutPlaneX();
    }

    public void ApplyCutY()
    {
        SetCutPlaneY();
    }

    public void ApplyCutZ()
    {
        SetCutPlaneZ();
    }

    private void TogglePlanarCut(PlanarTool tool, Vector3 localAxis)
    {
        if (activePlanarTool == tool && cutPlaneArmed && EditMode.currentMode == EditMode.Mode.Cut)
        {
            ExitCutMode();
            return;
        }

        LabelMode.labelMode = false;
        ArmPlanarCut(tool, localAxis);
    }

    private void ArmPlanarCut(PlanarTool tool, Vector3 localAxis)
    {
        EnterCutMode();
        activePlanarTool = tool;
        currentPlaneAxis = localAxis;
        cutPlaneArmed = true;
        hasHoverPoint = false;
        SnapPlaneToModel();
    }

    private void HandleShortcuts()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            EnterCutMode();
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            EnableFreeCut();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitCutMode();
            LabelMode.labelMode = false;
            InteractionMode.FreeCutActive = false;
            return;
        }

        if (EditMode.currentMode != EditMode.Mode.Cut)
            return;

        if (Input.GetKeyDown(KeyCode.X))
            SetCutPlaneX();

        if (Input.GetKeyDown(KeyCode.Y))
            SetCutPlaneY();

        if (Input.GetKeyDown(KeyCode.Z))
            SetCutPlaneZ();

        if (Input.GetKeyDown(KeyCode.Return) && hasHoverPoint)
            ApplyCut();
    }

    private void EnsureRuntimeSetup()
    {
        if (modelHolder == null)
        {
            GameObject holder = GameObject.Find("ModelHolder");
            if (holder != null)
                modelHolder = holder.transform;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (undoManager == null)
            undoManager = FindFirstObjectByType<MeshUndoManager>();

        if (patchManager == null)
            patchManager = FindFirstObjectByType<PatchManager>();

        if (cuttingPlaneObject == null)
            CreateCuttingPlane();

        if (cutExecutor == null)
            cutExecutor = GetComponent<CutExecutor>();

        if (cutExecutor == null)
            cutExecutor = gameObject.AddComponent<CutExecutor>();

        if (freeCutProcessor == null)
            freeCutProcessor = GetComponent<FreeCutProcessor>();

        if (freeCutDrawer == null)
            freeCutDrawer = GetComponent<FreeCutDrawer>();

        cutExecutor.modelHolder = modelHolder;
        cutExecutor.cuttingPlane = cuttingPlaneObject.transform;
    }

    private void EnsureToolButtonHighlighter()
    {
        if (GetComponent<ToolButtonHighlighter>() == null)
            gameObject.AddComponent<ToolButtonHighlighter>();
    }

    private Transform GetActiveModel()
    {
        if (modelHolder == null || modelHolder.childCount == 0)
            return null;

        for (int i = modelHolder.childCount - 1; i >= 0; i--)
        {
            Transform child = modelHolder.GetChild(i);
            if (child == null || !child.gameObject.activeInHierarchy)
                continue;

            if (child.GetComponent<MeshFilter>() != null)
                return child;
        }

        return null;
    }

    private bool TryGetHeartHit(out RaycastHit hit)
    {
        hit = default;

        Transform model = GetActiveModel();
        if (model == null || mainCamera == null)
            return false;

        MeshCollider collider = model.GetComponent<MeshCollider>();
        MeshFilter meshFilter = model.GetComponent<MeshFilter>();
        if (collider == null || meshFilter == null || meshFilter.sharedMesh == null)
            return false;

        collider.sharedMesh = null;
        collider.sharedMesh = meshFilter.sharedMesh;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        return collider.Raycast(ray, out hit, Mathf.Infinity);
    }

    private void NormalizeDefaultModelIfNeeded()
    {
        if (modelHolder == null)
        {
            GameObject holder = GameObject.Find("ModelHolder");
            if (holder != null)
                modelHolder = holder.transform;
        }

        if (modelHolder == null || modelHolder.childCount == 0)
            return;

        Transform sourceRoot = modelHolder.GetChild(0);
        if (sourceRoot == null || !sourceRoot.gameObject.activeSelf)
            return;

        if (sourceRoot.GetComponent<MeshFilter>() != null)
            return;

        MeshFilter sourceFilter = sourceRoot.GetComponentInChildren<MeshFilter>();
        MeshRenderer sourceRenderer = sourceRoot.GetComponentInChildren<MeshRenderer>();
        if (sourceFilter == null || sourceRenderer == null || sourceFilter.sharedMesh == null)
            return;

        Mesh bakedMesh = Instantiate(sourceFilter.sharedMesh);
        Matrix4x4 bakeMatrix = modelHolder.worldToLocalMatrix * sourceFilter.transform.localToWorldMatrix;

        Vector3[] vertices = bakedMesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = bakeMatrix.MultiplyPoint3x4(vertices[i]);
        bakedMesh.vertices = vertices;

        Vector3[] normals = bakedMesh.normals;
        if (normals != null && normals.Length == vertices.Length)
        {
            Matrix4x4 normalMatrix = bakeMatrix.inverse.transpose;
            for (int i = 0; i < normals.Length; i++)
                normals[i] = normalMatrix.MultiplyVector(normals[i]).normalized;
            bakedMesh.normals = normals;
        }
        else
        {
            bakedMesh.RecalculateNormals();
        }

        bakedMesh.RecalculateBounds();

        GameObject normalizedModel = new GameObject("DefaultHeartRuntime");
        normalizedModel.transform.SetParent(modelHolder, false);
        normalizedModel.transform.SetSiblingIndex(0);

        MeshFilter normalizedFilter = normalizedModel.AddComponent<MeshFilter>();
        MeshRenderer normalizedRenderer = normalizedModel.AddComponent<MeshRenderer>();
        MeshCollider normalizedCollider = normalizedModel.AddComponent<MeshCollider>();

        normalizedFilter.sharedMesh = bakedMesh;
        normalizedRenderer.sharedMaterials = sourceRenderer.sharedMaterials;
        normalizedCollider.sharedMesh = bakedMesh;

        sourceRoot.gameObject.SetActive(false);

        FreeCutProcessor processor = GetComponent<FreeCutProcessor>();
        if (processor != null)
            processor.targetHeart = normalizedModel;
    }

    private void SyncActiveModel()
    {
        Transform model = GetActiveModel();
        if (model == null)
            return;

        if (model != activeModel)
        {
            activeModel = model;
            CaptureCurrentModelAsOriginal();
            if (cutPlaneArmed)
                SnapPlaneToModel();
        }
    }

    public void RefreshSetup()
    {
        EnsureRuntimeSetup();
        SyncActiveModel();
        if (cutPlaneArmed)
            SnapPlaneToModel();
    }

    public void CaptureCurrentModelAsOriginal()
    {
        Transform model = GetActiveModel();
        if (model == null)
            return;

        MeshFilter mf = model.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return;

        originalModelMesh = Instantiate(mf.sharedMesh);
        originalMeshSource = model;
    }

    public void ResetModelGeometry()
    {
        Transform model = GetActiveModel();
        if (model == null || originalModelMesh == null)
            return;

        if (originalMeshSource != model)
            CaptureCurrentModelAsOriginal();

        MeshFilter mf = model.GetComponent<MeshFilter>();
        if (mf == null)
            return;

        Mesh restored = Instantiate(originalModelMesh);
        mf.mesh = restored;

        MeshCollider col = model.GetComponent<MeshCollider>();
        if (col != null)
        {
            col.sharedMesh = null;
            col.sharedMesh = restored;
        }

        if (patchManager != null)
            patchManager.ClearAllPatches();

        if (undoManager != null)
            undoManager.Clear();

        InteractionMode.FreeCutActive = false;
        LabelMode.labelMode = false;
        ExitCutMode();
        RefreshSetup();
    }

    private Vector3 GetModelAxis(Vector3 localAxis)
    {
        return localAxis.normalized;
    }

    private void CreateCuttingPlane()
    {
        cuttingPlaneObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        cuttingPlaneObject.name = CuttingPlaneName;
        cuttingPlaneObject.transform.SetParent(transform);
        Destroy(cuttingPlaneObject.GetComponent<Collider>());

        Material sourceMaterial = ResolveCutPlaneMaterial();
        MeshRenderer renderer = cuttingPlaneObject.GetComponent<MeshRenderer>();

        if (sourceMaterial != null)
        {
            Material runtimeMaterial = new Material(sourceMaterial);
            ConfigureCutPlaneMaterial(runtimeMaterial);
            renderer.material = runtimeMaterial;
        }
        else
        {
            renderer.enabled = false;
        }

        cuttingPlaneObject.SetActive(false);
    }

    private Material ResolveCutPlaneMaterial()
    {
        if (cutPlaneMaterial != null)
            return cutPlaneMaterial;

        if (runtimeCutPlaneTemplate != null)
            return runtimeCutPlaneTemplate;

        Shader fallbackShader = Shader.Find("Sprites/Default");
        if (fallbackShader == null)
            fallbackShader = Shader.Find("Unlit/Color");

        if (fallbackShader == null)
            return null;

        runtimeCutPlaneTemplate = new Material(fallbackShader);
        ConfigureCutPlaneMaterial(runtimeCutPlaneTemplate);
        return runtimeCutPlaneTemplate;
    }

    private void ConfigureCutPlaneMaterial(Material material)
    {
        if (material == null)
            return;

        if (material.HasProperty("_Surface"))
            material.SetFloat("_Surface", 1f);
        if (material.HasProperty("_Blend"))
            material.SetFloat("_Blend", 0f);
        if (material.HasProperty("_SrcBlend"))
            material.SetFloat("_SrcBlend", 5f);
        if (material.HasProperty("_DstBlend"))
            material.SetFloat("_DstBlend", 10f);
        if (material.HasProperty("_ZWrite"))
            material.SetFloat("_ZWrite", 0f);
        if (material.HasProperty("_Cull"))
            material.SetFloat("_Cull", 0f);

        material.renderQueue = 3000;
        material.color = new Color(1f, 0.2f, 0.2f, 0.25f);
    }
    private void SnapPlaneToModel()
    {
        if (cuttingPlaneObject == null)
            return;

        Transform model = GetActiveModel();
        if (model == null)
            return;

        MeshFilter mf = model.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return;

        Bounds bounds = mf.sharedMesh.bounds;
        Vector3 worldCenter = model.TransformPoint(bounds.center);
        Vector3 worldExtents = Vector3.Scale(bounds.extents, model.lossyScale);
        float size = Mathf.Max(worldExtents.x, worldExtents.y, worldExtents.z) * 2.2f;

        cuttingPlaneObject.transform.position = worldCenter;
        cuttingPlaneObject.transform.localScale = new Vector3(size, size, 1f);
        SetPlaneRotation(GetModelAxis(currentPlaneAxis));
    }

    private void MovePlaneToPoint(Vector3 worldPoint)
    {
        if (cuttingPlaneObject == null)
            return;

        Vector3 offsetDirection = mainCamera != null ? mainCamera.transform.forward : GetModelAxis(currentPlaneAxis);
        cuttingPlaneObject.transform.position = worldPoint + offsetDirection * PlaneSurfaceOffset;
        SetPlaneRotation(GetModelAxis(currentPlaneAxis));
    }

    private void SetPlaneRotation(Vector3 normal)
    {
        if (cuttingPlaneObject == null)
            return;

        Vector3 up = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.99f
            ? Vector3.forward
            : Vector3.up;

        cuttingPlaneObject.transform.rotation = Quaternion.LookRotation(normal, up);
        cuttingPlaneObject.transform.Rotate(0f, 180f, 0f);
    }

    public void PatchCut()
    {
        if (patchManager == null)
            return;

        patchManager.RestorePatch();
        ModelRotateZoom.rotationLocked = false;
    }
}