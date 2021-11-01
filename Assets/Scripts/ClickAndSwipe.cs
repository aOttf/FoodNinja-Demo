using UnityEngine;

[RequireComponent(typeof(TrailRenderer), typeof(BoxCollider), typeof(AudioSource))]
public class ClickAndSwipe : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ComboManager comboManager;
    private static Camera cam;
    private AudioSource hitData;

    //Components

    private TrailRenderer trail;
    private BoxCollider col;

    //Variables

    public bool isSwiping = false;
    public bool hasHit = false;

    private Vector3 curMousePos;
    private Vector3 lastMousePos;
    public Vector3 lastHitPos;

    //This is analogous to the velocity of the mouse movement
    private Vector3 VelMouse => (curMousePos - lastMousePos);

    private void Awake()

    {
        cam = Camera.main;
        trail = GetComponent<TrailRenderer>();
        col = GetComponent<BoxCollider>();
        hitData = GetComponent<AudioSource>();

        col.isTrigger = true;
        trail.enabled = false;
        col.enabled = false;
        hitData.playOnAwake = false;
        hitData.loop = false;
    }

    // Update is called once per frame

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            isSwiping = true;
        else if (Input.GetMouseButtonUp(0))
            isSwiping = false;

        UpdateComponents();
    }

    private void FixedUpdate()
    {
        UpdatecurMousePos();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && !gameManager.IsGameOver && VelMouse.magnitude > .01f)
        {
            AudioClip clip = other.gameObject.GetComponent<AudioSource>().clip;
            if (clip != null)
                hitData.PlayOneShot(clip);

            if (other.CompareTag("Food"))
            {
                lastHitPos = other.transform.position;
                gameManager.UpdateScore(1);
                comboManager.HasHit();
                comboManager.AddHit();
            }
            else if (other.CompareTag("Bomb"))
                gameManager.UpdateLive(-1);

            other.GetComponent<Target>().DestroyTarget();
        }
    }

    private void UpdatecurMousePos()
    {
        lastMousePos = curMousePos;
        curMousePos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
        transform.position = curMousePos;
    }

    private void UpdateComponents()
    {
        trail.enabled = col.enabled = isSwiping;
    }
}