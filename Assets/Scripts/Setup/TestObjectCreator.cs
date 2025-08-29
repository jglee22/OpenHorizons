using UnityEngine;

public class TestObjectCreator : MonoBehaviour
{
    [Header("Test Objects")]
    [SerializeField] private bool createTestObjects = true;
    [SerializeField] private int numberOfObjects = 5;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float minHeight = 0.5f;
    [SerializeField] private float maxHeight = 2f;
    
    [Header("Object Types")]
    [SerializeField] private bool createCubes = true;
    [SerializeField] private bool createSpheres = true;
    [SerializeField] private bool createCylinders = true;
    
    [Header("Materials")]
    [SerializeField] private Material[] testMaterials;
    
    private void Start()
    {
        if (createTestObjects)
        {
            CreateTestObjects();
        }
    }
    
    [ContextMenu("Create Test Objects")]
    public void CreateTestObjects()
    {
        Debug.Log("Creating test objects...");
        
        // Create different types of test objects
        for (int i = 0; i < numberOfObjects; i++)
        {
            CreateRandomTestObject(i);
        }
        
        Debug.Log($"Created {numberOfObjects} test objects!");
    }
    
    private void CreateRandomTestObject(int index)
    {
        // Random position within spawn radius
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        float randomHeight = Random.Range(minHeight, maxHeight);
        Vector3 position = new Vector3(randomCircle.x, randomHeight, randomCircle.y);
        
        // Random rotation
        Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        // Random object type
        PrimitiveType objectType = GetRandomObjectType();
        
        // Create the object
        GameObject testObject = GameObject.CreatePrimitive(objectType);
        testObject.name = $"TestObject_{index}_{objectType}";
        testObject.transform.position = position;
        testObject.transform.rotation = rotation;
        
        // Add InteractableObject script
        InteractableObject interactable = testObject.AddComponent<InteractableObject>();
        
        // Set random interaction prompt
        string[] prompts = {
            "고대 문서",
            "신비한 돌",
            "고대 유물",
            "수수께끼 상자",
            "고대 기둥",
            "신비한 문",
            "고대 제단",
            "수수께끼 장치"
        };
        
        interactable.SetInteractionPrompt(prompts[Random.Range(0, prompts.Length)]);
        
        // Set random material
        if (testMaterials != null && testMaterials.Length > 0)
        {
            Renderer renderer = testObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material randomMaterial = testMaterials[Random.Range(0, testMaterials.Length)];
                if (randomMaterial != null)
                {
                    renderer.material = randomMaterial;
                }
            }
        }
        
        // Add some random scale variation
        float scale = Random.Range(0.8f, 1.5f);
        testObject.transform.localScale = Vector3.one * scale;
        
        // Add collider if it doesn't exist
        if (testObject.GetComponent<Collider>() == null)
        {
            testObject.AddComponent<BoxCollider>();
        }
    }
    
    private PrimitiveType GetRandomObjectType()
    {
        if (createCubes && createSpheres && createCylinders)
        {
            PrimitiveType[] types = { PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Cylinder };
            return types[Random.Range(0, types.Length)];
        }
        else if (createCubes && createSpheres)
        {
            return Random.value > 0.5f ? PrimitiveType.Cube : PrimitiveType.Sphere;
        }
        else if (createCubes && createCylinders)
        {
            return Random.value > 0.5f ? PrimitiveType.Cube : PrimitiveType.Cylinder;
        }
        else if (createSpheres && createCylinders)
        {
            return Random.value > 0.5f ? PrimitiveType.Sphere : PrimitiveType.Cylinder;
        }
        else if (createCubes)
        {
            return PrimitiveType.Cube;
        }
        else if (createSpheres)
        {
            return PrimitiveType.Sphere;
        }
        else if (createCylinders)
        {
            return PrimitiveType.Cylinder;
        }
        
        return PrimitiveType.Cube;
    }
    
    [ContextMenu("Clear Test Objects")]
    public void ClearTestObjects()
    {
        GameObject[] testObjects = GameObject.FindGameObjectsWithTag("Untagged");
        int count = 0;
        
        foreach (GameObject obj in testObjects)
        {
            if (obj.name.StartsWith("TestObject_"))
            {
                DestroyImmediate(obj);
                count++;
            }
        }
        
        Debug.Log($"Cleared {count} test objects!");
    }
}

// Extension method to set interaction prompt
public static class InteractableObjectExtensions
{
    public static void SetInteractionPrompt(this InteractableObject interactable, string prompt)
    {
        var field = typeof(InteractableObject).GetField("interactionPrompt", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(interactable, prompt);
        }
    }
}
