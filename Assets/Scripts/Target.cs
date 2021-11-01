using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class Target : MonoBehaviour
{
    //Game Manager

    private static GameManager gameManager;
    public ParticleSystem explosionParticle;

    //Components

    private Rigidbody rb;
    private BoxCollider col;

    //Variables

    [SerializeField] private float maxSpeed = 17f;
    [SerializeField] private float minSpeed = 16f;
    [SerializeField] private float torqueRange = 4f;
    [SerializeField] private float xRange = 4f;
    [SerializeField] private float yPos = -6f;

    private Vector3 RandomForce => Random.Range(minSpeed, maxSpeed) * Vector3.up;
    private Vector3 RandomPosition => new Vector3(Random.Range(-xRange, xRange), yPos);
    private float RandomTorque => Random.Range(-torqueRange, torqueRange);

    // Start is called before the first frame update
    private void Start()
    {
        //Get components
        GetComponent<Component>();
        rb = GetComponent<Rigidbody>();

        //only allow motion in the x-y plane
        rb.constraints = RigidbodyConstraints.FreezePositionZ;
        col = GetComponent<BoxCollider>();

        //Get Game Objects
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        //Spawn
        RandomSpawn();
    }

    private void RandomSpawn()
    {
        //Add random speed to the target
        rb.AddForce(RandomForce, ForceMode.Impulse);
        //Add random torque to the target
        rb.AddTorque(RandomTorque, RandomTorque, RandomTorque, ForceMode.Impulse);
        //spawn the target at random position
        transform.position = RandomPosition;
    }

    //Destroy the target with particals when player swipes the target
    //Message called by PlayerSwiper
    public void DestroyTarget()
    {
        Instantiate(explosionParticle, transform.position, explosionParticle.transform.rotation);

        //Additional negative Influences when player hits a bomb besides losing one live
        if (CompareTag("Bomb"))
        {
            //Destroy all living foods
            var foods = GameObject.FindGameObjectsWithTag("Food");
            if (foods != null)
            {
                foreach (var food in foods)
                {
                    food.GetComponent<Target>().DestroyTarget();
                }
            }

            gameManager.BUlletTime4Bomb();
        }
        Destroy(gameObject);
    }
}