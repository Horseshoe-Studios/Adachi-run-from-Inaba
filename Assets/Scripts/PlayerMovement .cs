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

    [Header("Ajustes de Carriles")]
    [SerializeField] private float separacionCarriles = 2.5f;
    [SerializeField] private float velocidadCambioCarril = 20f; // Aumentada para que el cambio sea ágil e inmediato

    public Carriles PosicionActual = Carriles.centro;
    private int Carril = 2;

    private bool Shield_Active = false;
    private bool Shield_Can_Active = true;
    [SerializeField] private float Shield_Cooldown = 6f;
    [SerializeField] private float Shield_Duration = 2f;

    [SerializeField] private float MoveVelocity = 20f;
    Vector3 MoveDirection = Vector3.forward;

    [Header("Salto y Caída Rápida")]
    [SerializeField] private float JumpForce = 100f;
    [SerializeField] private float FastDropForce = 300f;
    [SerializeField] private float GravityUp = 250f;
    [SerializeField] private float GravityDown = 450f;
    private bool CanJump = true;

    private BoxCollider boxCollider;
    private float distanciaPivotAPies = 0f;

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

        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            distanciaPivotAPies = (boxCollider.size.y * 0.5f - boxCollider.center.y) * transform.lossyScale.y;

            if (boxCollider.sharedMaterial == null)
            {
                PhysicMaterial mat = new PhysicMaterial("SinFriccionAuto");
                mat.dynamicFriction = 0f;
                mat.staticFriction = 0f;
                mat.frictionCombine = PhysicMaterialCombine.Minimum;
                mat.bounceCombine = PhysicMaterialCombine.Minimum;
                boxCollider.material = mat;
            }
        }

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        AplicarSpriteSeleccionado();
        Coins = PlayerPrefs.GetInt(PREF_TOKENS, 0);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusicaJuego();
        }
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
        // 1. Movimiento lateral lineal a velocidad constante (sin ralentizarse al final)
        float Nueva_X_Lateral = Mathf.MoveTowards(rb.position.x, xObjetivo, velocidadCambioCarril * Time.fixedDeltaTime);

        // Snap: Si está a menos de 5cm del centro del carril, se clava exactamente en el objetivo
        if (Mathf.Abs(Nueva_X_Lateral - xObjetivo) < 0.05f)
        {
            Nueva_X_Lateral = xObjetivo;
        }

        // 2. Control vertical (Salto / Fast Drop)
        if (!CanJump)
        {
            if (rb.velocity.y < 0f)
            {
                float caidaEsteFrame = Mathf.Abs(rb.velocity.y) * Time.fixedDeltaTime;
                float alturaRayo = caidaEsteFrame + distanciaPivotAPies + 1.0f;

                RaycastHit hit;
                if (Physics.Raycast(rb.position + Vector3.up * 0.5f, Vector3.down, out hit, alturaRayo, 1 << 8))
                {
                    float yPiesActual = rb.position.y - distanciaPivotAPies;
                    float distanciaAlSuelo = yPiesActual - hit.point.y;

                    if (distanciaAlSuelo <= caidaEsteFrame)
                    {
                        rb.position = new Vector3(Nueva_X_Lateral, hit.point.y + distanciaPivotAPies, rb.position.z);
                        rb.velocity = new Vector3(0f, 0f, MoveDirection.z * MoveVelocity);
                        CanJump = true;
                    }
                }
            }

            if (!CanJump)
            {
                float gravity = rb.velocity.y > 0 ? GravityUp : GravityDown;
                rb.velocity += Vector3.down * gravity * Time.fixedDeltaTime;
                rb.position = new Vector3(Nueva_X_Lateral, rb.position.y, rb.position.z);
            }
        }
        else
        {
            rb.position = new Vector3(Nueva_X_Lateral, rb.position.y, rb.position.z);
        }

        // 3. Fijar rb.velocity.x estrictamente en 0f para evitar que la física pelee con el carril
        rb.velocity = new Vector3(0f, rb.velocity.y, MoveDirection.z * MoveVelocity);
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
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayPersonajeMover();
                    }
                    else if (dragDirection.x < 0 && Carril > 1)
                    {
                        Carril--;
                        if (AudioManager.Instance != null) AudioManager.Instance.PlayPersonajeMover();
                    }
                }
                else
                {
                    if (dragDirection.y > 0)
                    {
                        Salto();
                    }
                    else if (dragDirection.y < 0)
                    {
                        BajarSalto();
                    }
                }
            }
        }

        // PC
        if (Input.GetKeyDown(KeyCode.A) && Carril > 1)
        {
            Carril--;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayPersonajeMover();
        }
        if (Input.GetKeyDown(KeyCode.D) && Carril < 3)
        {
            Carril++;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayPersonajeMover();
        }
        if (Input.GetKeyDown(KeyCode.R) && !Shield_Active && Shield_Can_Active)
        {
            ActivateShield();
        }
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Salto();
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            BajarSalto();
        }
    }

    public void ActivateShield()
    {
        if (Shield_Active || !Shield_Can_Active) return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEscudoActivar();
        }

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

        rb.velocity = new Vector3(0f, JumpForce, rb.velocity.z);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPersonajeMover();
        }
    }

    private void BajarSalto()
    {
        if (CanJump) return;

        rb.velocity = new Vector3(0f, -FastDropForce, rb.velocity.z);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPersonajeMover();
        }
    }

    private void TakeDamage()
    {
        if (Shield_Active) return;
        vidas--;

        if (vidas <= 0)
        {
            Death();
        }
        else
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRecibirDano();
            }
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

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
            AudioManager.Instance.PlayPerder();
        }
    }

    private void TextUpdater()
    {
        string shield_String = Shield_Can_Active ? " Available" : " In cooldown!";
        CoinsText.text = "Coins: " + Coins + " $";
        LivesText.text = "Vidas: " + vidas;

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
        if (collision.gameObject.layer == 6)
        {
            TakeDamage();
        }
        else if (collision.gameObject.layer == 8)
        {
            CanJump = true;
            if (rb.velocity.y < 0f)
            {
                rb.velocity = new Vector3(0f, 0f, rb.velocity.z);
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 8)
        {
            CanJump = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == 8)
        {
            CanJump = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9)
        {
            Coins++;
            PlayerPrefs.SetInt(PREF_TOKENS, Coins);
            PlayerPrefs.Save();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMonedaRecoger();
            }

            other.gameObject.SetActive(false);
        }
    }
}