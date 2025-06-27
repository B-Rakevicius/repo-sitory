using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isEntrance = true;
    public bool isExit = false;

    void OnDrawGizmos()
    {
        Gizmos.color = isExit ? Color.red : (isEntrance ? Color.green : Color.yellow);
        Gizmos.DrawSphere(transform.position, 0.2f);
        Gizmos.DrawRay(transform.position, transform.forward * 1f);
    }
    /*
    private void Awake()
    {
        Debug.Log("this is " + this.name + " transforms " + this.transform.localRotation + " " + this.transform.localPosition );
    }
    */
}