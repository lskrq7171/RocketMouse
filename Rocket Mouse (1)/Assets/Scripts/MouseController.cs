using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MouseController : MonoBehaviour
{
    public float jetpackForce;
    public ParticleSystem jetpack;

    public float forwardMovementSpeed;

    private Rigidbody2D rb;

    public Transform groundCheckTransform;
    public LayerMask groundCheckLayerMask;
    private bool grounded;
    private Animator animator;

    private bool dead = false;

    private uint coins = 0;
    public TMP_Text textCoins;

    public GameObject buttonRestart;
    public GameObject buttonGoMenu;

    public AudioClip coinCollectSound;
    public AudioSource jetpackAudio;
    public AudioSource footstepsAudio;
    public AudioSource bgMusicAudio;

    public ParallaxScroll pallaxScroll;

    private int lv;
    public float lvUpInterval;
    private float lvUpTimeCnt;
    public TMP_Text textLv;

    public float FeverCtrl;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        textCoins.text = coins.ToString();

        lv = 1;
        textLv.text = $"Lv.{lv}";

        LoadVolume();

        StartCoroutine(FeverCtrl());
    }

    private void FixedUpdate()
    {
        bool jetpackActive = Input.GetButton("Fire1");

        if (!dead)
        {
            if (jetpackActive)
            {
                rb.AddForce(jetpackForce * Vector2.up);
            }

            Vector2 newVelocity = rb.velocity;
            newVelocity.x = forwardMovementSpeed;
            rb.velocity = newVelocity;
        }

        UpdateGroundStatus();
        AdjustJetpack(jetpackActive);
        DisplayButtons();
        AdjustFootstepsAndJetpackSound(jetpackActive);

        pallaxScroll.offset = transform.position.x;
    }

    private void Update()
    {
        lvUpTimeCnt += Time.deltaTime;
        if (lvUpTimeCnt >= lvUpInterval)
        {
            lv++;
            textLv.text = $"Lv.{lv}";
            forwardMovementSpeed += 0.5f;

            lvUpTimeCnt = 0;
        }
    }

    private void AdjustJetpack(bool jetpackActive)
    {
        var emission = jetpack.emission;
        emission.enabled = !grounded;
        emission.rateOverTime = jetpackActive ? 300f : 75f;
    }

    private void UpdateGroundStatus()
    {
        grounded = Physics2D.OverlapCircle(
            groundCheckTransform.position, 0.1f, groundCheckLayerMask);
        animator.SetBool("grounded", grounded);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Coins")
            CollectCoin(collision);
        else
        HitByLaser(collision);
    }

    private void CollectCoin(Collider2D coinCollider)
    {
        ++coins;
        textCoins.text = coins.ToString();
        
        Destroy(coinCollider.gameObject);

        AudioSource.PlayClipAtPoint(coinCollectSound, transform.position);
    }
    
    private void HitByLaser(Collider2D laserCollider)
    {
        if (!dead)
        {
            AudioSource laser = laserCollider.GetComponent<AudioSource>();
            laser.Play();
        }
        
        dead = true;
        animator.SetBool("dead", true);
    }

    private void DisplayButtons()
    {
        bool active = buttonRestart.activeSelf;
        if (grounded && dead && !active)
        {
            buttonRestart.SetActive(true);
            buttonGoMenu.SetActive(true);
        }
    }

    public void OnClickedRestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnClickedGoMenuButton()
    {
        SceneManager.LoadScene("Menu");
    }
    private void AdjustFootstepsAndJetpackSound(bool jetpackActive)
    {
        footstepsAudio.enabled = !dead && grounded;
        jetpackAudio.enabled = !dead && !grounded;
        jetpackAudio.volume = jetpackActive ? 1f : 0.5f;
    }

    private void LoadVolume()
    {
        float volume = PlayerPrefs.GetFloat("bgVolume");
        bgMusicAudio.volume = volume;
    }

    IEnumerator FeverCount()
    {
        while (true)
        {
            yield return new WaitForSeconds(feverInterval);
            Debug.Log("레벨 증가");
        }
    }
}