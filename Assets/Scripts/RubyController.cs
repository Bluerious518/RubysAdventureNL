using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RubyController : MonoBehaviour
{
    public float speed = 3.0f;

    public int maxHealth = 5;
    public float timeInvincible = 2.0f;
    
    int currentHealth;
    public int health { get { return currentHealth; }}

    bool isInvincible;
    float invincibleTimer;
    public ParticleSystem hurtEffect;
    public ParticleSystem healEffect;
    public ParticleSystem speedEffect;

    bool isSpeed;
    float speedTimer;
    public float timeSpeed = 5.0f;

    bool isDelay;
    float delayTimer;
    public float timeDelay = 2.0f;

    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;

    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);

    public GameObject projectilePrefab;

    AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip throwSound;
    public AudioClip fixSound;
    public AudioClip winSound;
    public AudioClip loseSound;

    public int score;
    public GameObject scoreTextObject;
    TextMeshProUGUI scoreText;

    public GameObject gameOverText;
    bool gameOver;
    bool gameWin;


    // Start is called before the first frame update
    void Start()
    {
        gameOver = false;
        gameWin = false;
        rigidbody2d = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    
        audioSource= GetComponent<AudioSource>();

        TextMeshProUGUI scoreTextComponent = scoreTextObject.GetComponentInChildren<TextMeshProUGUI>();

        if (scoreTextComponent != null)
        {
             scoreText = scoreTextComponent;
        }
        else
        {
            Debug.LogError("No TextMeshProUGUI component found in the children of scoreTextObject.");
        }    
    }

    // Update is called once per frame
    void Update()
    {

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        if (isSpeed)
        {
            speedTimer -= Time.deltaTime;

            if (speedTimer < 0)
            {
                isSpeed = false;
                speed = Mathf.Max(speed-2.0f, 0.0f);
            }
        }

        if(Input.GetKeyDown(KeyCode.C))
        {
            if (gameOver == false)
            {
                Launch();
            }
        }

        if (isDelay)
        {
            delayTimer -= Time.deltaTime;

            if (delayTimer <= 0)
            isDelay = false;
        }

        if(Input.GetKeyDown(KeyCode.X))
        {
            if (gameOver == false)
            {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
            if(hit.collider != null)
            {
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                if (character != null)
                {
                    character.DisplayDialog();
                }
            }
            }
        }

        if (currentHealth == 0 && !gameOver)
        {
            gameOverText.SetActive(true);
            gameOverText.GetComponent<TextMeshProUGUI>().SetText("You lost! Press R to Restart!");
            gameOver = true;
            speed = 0.0f;
            PlaySound(loseSound);
        }

        if (score == 3 && !gameWin)
        {
            gameOverText.SetActive(true);
            gameOverText.GetComponent<TextMeshProUGUI>().SetText("You win! Game created by Group 32.");
            speed = 0.0f;
            gameWin = true;
            PlaySound (winSound);
        }

        if (Input.GetKey(KeyCode.R))
            {
                if (gameOver == true)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
    }

    void FixedUpdate()
    {
        
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime; 
        position.y = position.y + speed * vertical * Time.deltaTime;
        
        rigidbody2d.MovePosition(position);
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            animator.SetTrigger("Hit");

            if (isInvincible)
                return;
            
            isInvincible = true;
            invincibleTimer = timeInvincible;
            Instantiate(hurtEffect, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

            PlaySound(hitSound);
        }

        if (amount > 0)
        {
            Instantiate(healEffect, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
    }

    public void ChangeSpeed (int speedAmount)
    {
        if (speedAmount > 0)
        {
            Instantiate(speedEffect, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity, rigidbody2d.transform);
            speedTimer = timeSpeed;
            if (isSpeed == true)
            return;
            isSpeed = true;
            speed = (speed + speedAmount);
            
        }
    }

    public void ChangeScore(int scoreAmount)
    {
        if (scoreAmount > 0)
        {
            score = score + scoreAmount;
            PlaySound(fixSound);
        } 

        if (scoreText != null)
        {
            scoreText.text = "Fixed Robots: " + score.ToString();
        }
        else
        {
            Debug.LogError("scoreText is not assigned. Make sure scoreTextObject has a TextMeshProUGUI component.");
        }
        
    }

    void Launch()
    {
        
        if (isDelay)
        return;
        isDelay = true;
        delayTimer = timeDelay;  
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);

        animator.SetTrigger("Launch");

        PlaySound(throwSound);  
        

    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
