using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

// Requires a renderer, mesh filter and material
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCombine : SerializedMonoBehaviour
{
    #region Field Declarations

    #region Editor Values
    [Header("Runtime Options")]
    [SerializeField]
    [Tooltip("If this is enabled, the mesh will be Combined during the start event, using the Run Mode")]
    private bool combineOnStart = false;

    [SerializeField]
    [Tooltip("Whether to combine using single material mode or multi material mode")]
    [EnableIf("combineOnStart")]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 40)]
    private RunModes runMode = RunModes.SingleMaterial;

    // Whether meshes are already combined
    private bool meshesAreCombined;

    #endregion
    #region Core Values

    // Mesh filter cache
    private MeshFilter containerMeshFilter;
    private MeshRenderer containerRenderer;
    private MeshFilter[] childrenMeshFilters;
    private CombineInstance[] finalCombineInstances;

    #endregion

    #endregion

    #region Unity Methods

    // Reset is used when the component is reset or placed for the first time
    private void Reset()
    {
        // Caches common references
        CacheReferences();

        if (transform.position != new Vector3(0f, 0f, 0f))
        {
            Debug.LogWarning("The container must be on the world origin for the combine to work properly!");
        }
    }

    // Runs before the first frame
    private void Start()
    {
        // If combine on start has been enabled
        if (combineOnStart)
        {
            // Select operation based on runmode
            switch (runMode)
            {
                case RunModes.MultiMaterial:
                    MultiMaterialCombine();
                    break;
                case RunModes.SingleMaterial:
                    SingleMaterialCombine();
                    break;
                default:
                    SingleMaterialCombine();
                    Debug.Log("Defaulted to single material mode!");
                    break;
            }
        }
    }
    #endregion

    #region Custom Methods
    [InfoBox("Use the Combine with multiple materials button to combine meshes that use more than one material. Use the combine with single material to combine meshes with only one material. The latter is faster. In-editor mesh combination may not generate meshes that exceed 65,535 vertices. If such a mesh is generated, deformation may occur. You may use the Uncombine meshes button to revert the change.", InfoMessageType.Info)]
    [HideIf("combineOnStart")]
    [DisableIf("meshesAreCombined")]
    [Button("Combine with multiple materials", ButtonSizes.Large)]
    // FIXME: This most of this method is horrendously unoptimized, optimize it later
    // Combines meshes with differing materials into a parent mesh subdivided into submeshes
    public void MultiMaterialCombine()
    {
        // Caches frequently used references and components
        CacheReferences();

        // The output mesh where all other final submeshes are combined
        Mesh outputMesh = new Mesh();

        // Creates a new list to store the materials
        List<Material> materials = new List<Material>();
        // Creates an array of submeshes. Each submesh will contain a material
        Mesh[] subMeshes;
        // Creates a list of sublists of combine instances for each material
        // FIXME: Doing this is horrendous for performance
        List<List<CombineInstance>> combineInstanceLists = new List<List<CombineInstance>>();

        // HACK: To avoid annoying matrix math, we instead are resetting the position and restoring it later
        Quaternion rotation = transform.rotation;
        Vector3 position = transform.position;

        // For each of the children mesh filters
        foreach (MeshFilter meshFilter in childrenMeshFilters)
        {
            // Gets the mesh renderer attached to the mesh filter
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            // Skips if it is the container
            if (meshFilter == containerMeshFilter) continue;
            // Skips if the mesh renderer is null or the meshfilter's shared mesh is null or the mesh renderer's material count is different from the mesh filter's submesh count
            // This is important because each submesh is a material; therefore, if there are more materials than submeshes, the mesh combine will fail
            if (!meshRenderer || !meshFilter.sharedMesh || meshRenderer.sharedMaterials.Length != meshFilter.sharedMesh.subMeshCount) continue;

            // For each submesh in the meshfilter's mesh
            for (int sharedMaterialIndex = 0; sharedMaterialIndex < meshFilter.sharedMesh.subMeshCount; sharedMaterialIndex++)
            {
                // If the material does not exist in the materials list
                if (!materials.Contains(meshRenderer.sharedMaterials[sharedMaterialIndex]))
                {
                    // Add the material to the materials list
                    materials.Add(meshRenderer.sharedMaterials[sharedMaterialIndex]);
                }

                // Instantiates the material sublist
                combineInstanceLists.Add(new List<CombineInstance>());

                // Creates a new combine instance to store data about the mesh
                CombineInstance combineInstance = new CombineInstance();

                // Populates combineInstance with data
                combineInstance.transform = meshRenderer.transform.localToWorldMatrix;
                combineInstance.subMeshIndex = sharedMaterialIndex;
                combineInstance.mesh = meshFilter.sharedMesh;

                // Adds the combine instance to the material sublist
                combineInstanceLists[sharedMaterialIndex].Add(combineInstance);
            }

            // Disables the child's meshrenderer
            meshRenderer.enabled = false;
        }

        // Combine material index into a per-material mesh
        // Initializes the submesh array
        subMeshes = new Mesh[materials.Count];
        // Creates a new combine instance for each material
        CombineInstance[] finalCombineInstances = new CombineInstance[materials.Count];

        // For each material in materials
        for (int sharedMaterialIndex = 0; sharedMaterialIndex < materials.Count; sharedMaterialIndex++)
        {
            // Generates an array of combine instances that will be used to generate each submesh
            CombineInstance[] subfinalCombineInstances = combineInstanceLists[sharedMaterialIndex].ToArray();

            // Assigns a new mesh to the submesh array entry
            subMeshes[sharedMaterialIndex] = new Mesh();
            // Combines the meshes in the sub combine instances array
            subMeshes[sharedMaterialIndex].CombineMeshes(subfinalCombineInstances);

            // Assigns a new combine index to the final combine instances array
            finalCombineInstances[sharedMaterialIndex] = new CombineInstance();
            // Gives it the combined mesh of the subcombiners
            finalCombineInstances[sharedMaterialIndex].mesh = subMeshes[sharedMaterialIndex];
            // Set submesh index to 0, since we no longer need to worry about each submesh containing a material
            finalCombineInstances[sharedMaterialIndex].subMeshIndex = 0;
        }

        // Combine submeshes into output mesh
        // mergeSubMeshes submeshes is set to false, because we need them to display individual materials
        // useMatrices is set to false because we want to ignore the transform of the submeshes
        outputMesh.CombineMeshes(finalCombineInstances, false, false);

        // Sets the container's mesh to the output mesh
        containerMeshFilter.sharedMesh = outputMesh;
        containerMeshFilter.sharedMesh.name = "Combined mesh";

        // Assigns materials to container mesh renderer
        containerRenderer.sharedMaterials = materials.ToArray();

        // Warns the developer the combined mesh exceeds unity's vertex limit
        if (outputMesh.vertexCount >= 65535)
        {
            Debug.LogError("The combined mesh exceeds Unity's limit of 65,535 vertices per mesh! Try combining fewer meshes!");
            Debug.Log("Automatically uncombined meshes.");
            Uncombine();
        }

        // Resets rotation and position
        transform.rotation = Quaternion.identity;
        transform.position = transform.position;

        // Marks meshes as combined
        meshesAreCombined = true;
    }

    [HideIf("combineOnStart")]
    [DisableIf("meshesAreCombined")]
    [Button("Combine with single material", ButtonSizes.Large)]
    // combines all meshes using a single material
    public void SingleMaterialCombine()
    {
        // Caches frequently used references and components
        CacheReferences();

        // Sets the renderer's material to the material of the first child
        containerRenderer.sharedMaterial = transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;

        // The output mesh where all other meshes are combined
        Mesh outputMesh = new Mesh();

        //HACK: To avoid annoying matrix math, we instead are resetting the position and restoring it later
        Quaternion rotation = transform.rotation;
        Vector3 position = transform.position;


        // Resets rotation and position
        transform.rotation = Quaternion.identity;
        transform.position = transform.position;

        // Iterates though the combine instances
        for (int index = 0; index < childrenMeshFilters.Length; index++)
        {
            // Skips over the mesh if it is the container's mesh
            if (childrenMeshFilters[index].transform == transform) continue;

            // Checks if the meshfilter is null first to prevent warnings
            if (childrenMeshFilters[index] != null)
            {
                // Since we're not using sub meshes, set the submesh index to 0
                finalCombineInstances[index].subMeshIndex = 0;
                // Sets the combine instance's mesh to the mesh filter's shared mesh
                finalCombineInstances[index].mesh = childrenMeshFilters[index].sharedMesh;
                // Converts the transform's coordinates to a world position matrix
                finalCombineInstances[index].transform = childrenMeshFilters[index].transform.localToWorldMatrix;
            }
        }

        // Combines the combine intances into a final mesh
        outputMesh.CombineMeshes(finalCombineInstances);

        // Warns the developer the combined mesh exceeds unity's vertex limit
        if (outputMesh.vertexCount >= 65535)
        {
            Debug.LogError("The combined mesh exceeds Unity's limit of 65,535 vertices per mesh! Try combining fewer meshes!");
            Debug.Log("Automatically uncombined meshes.");
            Uncombine();
        }

        // Sets the container's mesh filter to display the output mesh
        containerMeshFilter.sharedMesh = outputMesh;
        containerMeshFilter.sharedMesh.name = "Combined mesh";

        // Restores rotation and position
        // Rotation is always done first because rotatin after placing it may move the model, depending on the center of rotation.
        transform.rotation = rotation;
        transform.position = position;

        // Disables all the mesh renderers 
        for (int index = 0; index < transform.childCount; index++)
        {
            // Disables rendering on the original gameobject
            transform.GetChild(index).GetComponent<MeshRenderer>().enabled = false;
        }

        // Marks meshes are combined
        meshesAreCombined = true;
    }

    [Button("Uncombine meshes", ButtonSizes.Large)]
    // Uncombines all meshes
    public void Uncombine()
    {
        // Caches commonly used references
        CacheReferences();

        // Sets the mesh to none
        containerMeshFilter.mesh = null;
        // Sets the mesh renderer to only have one material
        containerRenderer.materials = new Material[1];
        // Clears the renderer's materials
        containerRenderer.material = null;

        // Re-enables all the mesh renderers 
        for (int index = 0; index < transform.childCount; index++)
        {
            // Disables rendering on the original gameobject
            transform.GetChild(index).GetComponent<MeshRenderer>().enabled = true;
        }

        // Marks meshes as uncombined
        meshesAreCombined = false;
    }

    // Caches common references throughout the code
    private void CacheReferences()
    {
        // Container mesh filter and renderer
        containerMeshFilter = GetComponent<MeshFilter>();
        containerRenderer = GetComponent<MeshRenderer>();

        // Common references throughout code
        childrenMeshFilters = GetComponentsInChildren<MeshFilter>();
        finalCombineInstances = new CombineInstance[childrenMeshFilters.Length];
    }
    #endregion
}

// The run modes for mesh merging
public enum RunModes
{
    SingleMaterial,
    MultiMaterial,
    /*SingleMaterialIncremental,
    MultiMaterialIncremental,
    SingleMaterialMultithreaded,
    MultiMaterialMultithreaded,
    SingleMaterialIncMultithreaded,
    MultiMaterialIncMultithreaded*/
}