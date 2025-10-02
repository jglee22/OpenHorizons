using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Target/LocationTarget", fileName = "Location Target")]
public class LocationTarget : TaskTarget
{
    [Header("Location Settings")]
    [SerializeField] public Vector3 targetPosition;
    [SerializeField] public float reachDistance = 5f;
    [SerializeField] public string locationName = "";
    
    public override object Value => targetPosition;

    public override bool IsEqual(object target)
    {
        if (target is Vector3 position)
        {
            float distance = Vector3.Distance(position, targetPosition);
            return distance <= reachDistance;
        }
        
        if (target is string name)
        {
            return name == locationName;
        }
        
        return false;
    }
    
    public Vector3 TargetPosition => targetPosition;
    public float ReachDistance => reachDistance;
    public string LocationName => locationName;
}
