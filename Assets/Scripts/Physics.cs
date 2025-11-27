using UnityEngine;

[RequireComponent(typeof(CompositeCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class AsignarPhysicsMaterial : MonoBehaviour
{
    void Start()
    {
        CompositeCollider2D composite = GetComponent<CompositeCollider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // Asegurar configuración del Rigidbody2D en Kinematic
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Crear un nuevo PhysicsMaterial2D con friction 0 y bounce 0
        PhysicsMaterial2D physMat = new PhysicsMaterial2D("ZeroFriction")
        {
            friction = 0f,
            bounciness = 0f
        };

        // Asignar el material al CompositeCollider2D
        composite.sharedMaterial = physMat;

        Debug.Log("PhysicsMaterial2D asignado al CompositeCollider2D con friction 0 y bounciness 0.");
    }
}