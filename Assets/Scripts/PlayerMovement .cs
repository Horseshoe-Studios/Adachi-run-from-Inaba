using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private const string PREF_TOKENS = "TokensGuardados";

    [Header("Visual del Personaje")]
    [SerializeField] private GameObject Adachi;
    [SerializeField] private GameObject Shield_Object;
    [SerializeField] private MeshRenderer Shield_Mesh_Renderer;
    [SerializeField] private Rigidbody rb;

    // Controles móvil
    [SerializeField] private float minSwipeDistance = 50f;
    private Vector2 dragStartPosition;

    private int playerLayer;
    private int obstaculosLayer;

    public enum Carriles
    {
        izquierdo = 1,
        centro,
        derecha,
    }

    [SerializeField] private int vidas = 3;
    private int Coins = 0;
    private float Timer = 0;
    private float NewTimer = 0;

    [SerializeField] private float separacionCarriles = 2.5f;
    [SerializeField] private float velocidadCambioCarril = 15f;

    public Carriles PosicionActual = Carriles.centro;
    private int Carril = 2;

    private bool Shield_Active = false;
    private bool Shield_Can_Active = true;
    [SerializeField] private float Shield_Cooldown = 6f;
    [SerializeField] private float Shield_Duration = 2f;

    [SerializeField] private float MoveVelocity = 20f;
    Vector3 MoveDirection = Vector3.forward;

    [SerializeField] private float JumpForce = 100f;
    [SerializeField] private float GravityUp = 250f;
    [SerializeField] private float GravityDown = 450f;
    private bool CanJump = true;

    private float xCentro;
    private float xObjetivo;

    private float DamagedTimeWindow = 0.5f;
    [SerializeField] private GameObject Death_UI;
    [SerializeField] private GameObject Basic_UI;

    [SerializeField] private TextMeshProUGUI LivesText;
    [SerializeField] private TextMeshProUGUI TimerText;
    [SerializeField] private TextMeshProUGUI Timer_Death_Text;
    [SerializeField] private TextMeshProUGUI Shield_CD_Text;
    [SerializeField] private TextMeshProUGUI CoinsText;

    void Start()
    {
        xCentro = rb.position.x;
        xObjetivo = xCentro;

        playerLayer = LayerMask.NameToLayer("Player");
        obstaculosLayer = LayerMask.NameToLayer("Obstaculo");

        // 1. APLICAR EL SPRITE SELECCIONADO EN EL MENÚ
        AplicarSpriteSeleccionado();

        // 2. CARGAR MONEDAS GUARDADAS
        Coins = PlayerPrefs.GetInt(PREF_TOKENS, 0);
    }

    private void AplicarSpriteSeleccionado()
    {
        if (Adachi != null && MenuManager.spriteSeleccionado != null)
        {
            SpriteRenderer sr = Adachi.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = MenuManager.spriteSeleccionado;
            }
        }
    }

    void Update()
    {
        Inputs();
        Posicionamiento_Jugador();
        TextUpdater();
        Timer += 1 * Time.deltaTime;
        NewTimer = Mathf.RoundToInt(Timer);
    }

    private void FixedUpdate()
    {
        float Nueva_X_Lateral = Mathf.Lerp(rb.position.x, xObjetivo, velocidadCambioCarril * Time.fixedDeltaTime);
        rb.position = new Vector3(Nueva_X_Lateral, rb.position.y, rb.position.z);
        if (!CanJump)
        {
            float gravity = rb.velocity.y > 0 ? GravityUp : GravityDown;
            rb.velocity += Vector3.down * gravity * Time.fixedDeltaTime;
        }

        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, MoveDirection.z * MoveVelocity);
    }

    private void Inputs()
    {
        // Móvil
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 dragEndPosition = Input.mousePosition;
            Vector2 dragDirection = dragEndPosition - dragStartPosition;

            if (dragDirection.magnitude >= minSwipeDistance)
            {
                if (Mathf.Abs(dragDirection.x) > Mathf.Abs(dragDirection.y))
                {
                    if (dragDirection.x > 0 && Carril < 3)
                    {
                        Carril++;
                    }
                    else if (dragDirection.x < 0 && Carril > 1)
                    {
                        Carril--;
                    }
                }
                else
                {
                    if (dragDirection.y > 0)
                    {
                        Salto();
                    }
                }
            }
        }

        // PC
        if (Input.GetKeyDown(KeyCode.A) && Carril > 1)
        {
            Carril--;
        }
        if (Input.GetKeyDown(KeyCode.D) && Carril < 3)
        {
            Carril++;
        }
        if (Input.GetKeyDown(KeyCode.R) && !Shield_Active && Shield_Can_Active)
        {
            ActivateShield();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Salto();
        }
    }

    public void ActivateShield()
    {
        if (Shield_Active || !Shield_Can_Active) return;
        Debug.Log("escudo activado");
        Shield_Active = true;
        Shield_Can_Active = false;
        StartCoroutine(Shield());
    }

    private void Posicionamiento_Jugador()
    {
        float offsetX = 0f;

        switch (Carril)
        {
            case 1:
                PosicionActual = Carriles.izquierdo;
                offsetX = -separacionCarriles;
                break;

            case 2:
                PosicionActual = Carriles.centro;
                offsetX = 0f;
                break;

            case 3:
                PosicionActual = Carriles.derecha;
                offsetX = separacionCarriles;
                break;
        }

        xObjetivo = xCentro + offsetX;
    }

    private void Salto()
    {
        if (!CanJump) return;
        CanJump = false;

        rb.velocity = new Vector3(rb.velocity.x, JumpForce, rb.velocity.z);
    }

    private void TakeDamage()
    {
        if (Shield_Active) return;
        vidas--;

        if (vidas <= 0)
        {
            Death();
        }
        StartCoroutine(Damaged());
    }

    IEnumerator Damaged()
    {
        if (vidas <= 0) yield break;

        Physics.IgnoreLayerCollision(playerLayer, obstaculosLayer, true);
        yield return new WaitForSeconds(DamagedTimeWindow);
        Physics.IgnoreLayerCollision(playerLayer, obstaculosLayer, false);
    }

    private void Death()
    {
        Time.timeScale = 0;
        Death_UI.SetActive(true);
        Basic_UI.SetActive(true);
    }

    private void TextUpdater()
    {
        string shield_String = Shield_Can_Active ? " Available" : " In cooldown!";
        CoinsText.text = "Coins: " + Coins + " $";
        LivesText.text = "Lives: " + vidas;

        Shield_CD_Text.text = "Shield: " + shield_String;
        if (vidas <= 0)
        {
            Timer_Death_Text.text = "Timer: " + NewTimer;
        }
        else
        {
            TimerText.text = "Timer: " + NewTimer;
        }
    }

    private IEnumerator Shield()
    {
        Color Shield_Color = Shield_Mesh_Renderer.material.color;
        Shield_Color.a = 0.4f;
        Shield_Object.SetActive(true);

        Physics.IgnoreLayerCollision(playerLayer, obstaculosLayer, true);

        yield return new WaitForSeconds(Shield_Duration);

        Shield_Active = false;
        Physics.IgnoreLayerCollision(playerLayer, obstaculosLayer, false);
        Shield_Object.SetActive(false);

        yield return new WaitForSeconds(Shield_Cooldown);
        Shield_Can_Active = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 6) // capa de obstáculos
        {
            TakeDamage();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 8) CanJump = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 8) CanJump = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9) // capa de monedas
        {
            Coins++;
            // Guardar inmediatamente la moneda en el almacén de tokens
            PlayerPrefs.SetInt(PREF_TOKENS, Coins);
            PlayerPrefs.Save();

            other.gameObject.SetActive(false);
        }
    }
}