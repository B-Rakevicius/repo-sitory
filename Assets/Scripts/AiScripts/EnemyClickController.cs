using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

/// <summary>
/// early enemy movement troubleshooting class, delete later.
/// moves a rigidbody nav_mesh_agent that this class is attached to.
/// </summary>
public class EnemyClickController : MonoBehaviour
{
    public Camera camera;
    public NavMeshAgent agent;
    private Mouse mouse;
    void Start()
    {
        mouse = Mouse.current;
    }
    void Update()
    {
        if (mouse.leftButton.wasPressedThisFrame)
        {
            Ray ray = camera.ScreenPointToRay(mouse.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(hit.point);
            }
        }
    }
}