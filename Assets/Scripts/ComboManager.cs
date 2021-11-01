using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ComboManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ClickAndSwipe clickAndSwipe;
    private AudioSource hitComboData;

    [SerializeField] private GameObject comboUI;
    [SerializeField] private TextMeshProUGUI comboText;
    private float UIDistFromCam;

    private UnityEvent outOfComboEvent;
    private UnityEvent inToComboEvent;

    [SerializeField] private float maxTimeBetweenHits = 0.04f;
    [SerializeField] private int hitsUntilCombo = 3;

    //private Queue<float> hitTimeInACombo;
    private Vector3 lastHitWorldPos;

    private float lastHitTime;
    private int hitCount = 0;

    private bool inCombo;
    private bool hasHit;
    private bool IsMouseDown => clickAndSwipe.isSwiping;
    private bool IsComboTimedOut => (Time.time - lastHitTime > maxTimeBetweenHits);
    private bool IsOutOfCombo => inCombo && (!IsMouseDown || IsComboTimedOut);
    private bool IsIntoCombo => !inCombo && hasHit;

    private void Start()
    {
        hitComboData = GetComponent<AudioSource>();

        UIDistFromCam = comboUI.GetComponent<Canvas>().planeDistance;
        comboUI.SetActive(false);

        if (outOfComboEvent == null)
            outOfComboEvent = new UnityEvent();
        if (inToComboEvent == null)
            inToComboEvent = new UnityEvent();
        outOfComboEvent.AddListener(OutOfComboListeners);
        inToComboEvent.AddListener(IntoComboLinsteners);

        //hitTimeInACombo = new Queue<float>(hitsUntilCombo - 2);
    }

    private void Update()
    {
        if (IsOutOfCombo && outOfComboEvent != null)
            outOfComboEvent.Invoke();
        if (IsIntoCombo && inToComboEvent != null)
            inToComboEvent.Invoke();
    }

    //------------------Message Funcions Called by Swiper-----------------------//
    public void AddHit() => hitCount++;

    public void HasHit() => hasHit = true;

    //-----------------------------------------------------------------------------//

    //----------------------------Listeners for going into and outof Combo State------------//
    private void OutOfComboListeners()
    {
        ShowComboInfo();
        ResetCombo();
    }

    private void IntoComboLinsteners()
    {
        SetCombo();
    }

    private void ShowComboInfo()
    {
        if (hitCount > hitsUntilCombo)
        {
            StartCoroutine("ShowComboText");
            gameManager.UpdateScore(hitCount);
            if (hitComboData.clip != null)
                hitComboData.PlayOneShot(hitComboData.clip);
        }
    }

    private IEnumerator ShowComboText()
    {
        comboUI.SetActive(true);
        comboText.text = hitCount + " Combo!";
        lastHitWorldPos = clickAndSwipe.lastHitPos;
        comboText.transform.position =
            new Vector3(lastHitWorldPos.x, lastHitWorldPos.y, UIDistFromCam + Camera.main.transform.position.z);

        yield return new WaitForSeconds(0.8f);
        comboUI.SetActive(false);
    }

    private void ResetCombo()
    {
        inCombo = false;
        hasHit = false;
        lastHitTime = 0f;
        hitCount = 0;
    }

    private void UpdateHitTimeQueue()
    {
    }

    private void SetCombo()
    {
        inCombo = true;
        lastHitTime = Time.time;
        hitCount = 1;
    }

    //--------------------------------------------------------------//
}