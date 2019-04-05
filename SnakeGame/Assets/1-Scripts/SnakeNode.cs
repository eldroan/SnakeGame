using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class SnakeNode : MonoBehaviour
{
    [Header("Valores de configuración")]
    [SerializeField]  private float tickRate = 1f;
    [SerializeField] private float movingDistance;
    [SerializeField] private GameObject snakeNodePrefab;
    [SerializeField] private GameObject fruitPrefab;
    [SerializeField] private int maxX,minX,maxZ,minZ;
    public UnityEvent fruitEaten;

    private float _timeToNextTick;
    private static bool _firstNode = true;

    [Header("Valores del nodo")]
    [SerializeField] private bool _isHead;
    [SerializeField] private SnakeNode _next = null;
    [SerializeField] private Vector3 _lookingDirection;
    [SerializeField] private bool shouldSpawn;

    private void Awake()
    {
        if (_firstNode)
        {
            _isHead = true; //Si soy el primer nodo en ser creado me seteo como la cabeza de la viborita
            _firstNode = false;
            var myRigidbody = this.gameObject.AddComponent<Rigidbody>(); //La cabeza registra las colisiones, por esto necesita un rigidbody. Solo objetos que posean un rigidbody reciben mensajes del tipo OnCollisionXXX u OnTriggerXXX
            myRigidbody.isKinematic = true; //Si un rigidbody es seteado como kinematico entonces no sera afectado por la gravedad y en este curso no se respetan las leyes de la fisica
            _timeToNextTick = tickRate;
            _lookingDirection = Vector3.right;
            
            //Hay que spawnear la fruta y colocarla en algún lugar aleatorio del tablero
            Instantiate(fruitPrefab, this.GenerateRandomFruitPosition(), Quaternion.identity);
        }
    }

    private Vector3 GenerateRandomFruitPosition()
    {
        Vector3 currentHeadPos = this.transform.position;
        Vector3 fruitPos;

        do
        {
            fruitPos = new Vector3(UnityEngine.Random.Range(minX, maxX), 0f, UnityEngine.Random.Range(minZ, maxX));
        } while (fruitPos.Equals(currentHeadPos));

        return fruitPos;
    }

    void Update()
    {
        if (_isHead) //La cabeza de la serpiente es la que maneja el input del player
        {
            _timeToNextTick -= Time.deltaTime; //Time.deltaTime devuelve el tiempo en segundos que paso desde el último frame, nos permite contar tiempo de manera independiente del framerate del juego

            if (_timeToNextTick < 0)
            {
                //Hay que consultar que tecla esta presionando el player:
                // - Si no esta presionando ninguna mantenemos la direccion previa de la viborita
                // - La viborita no puede moverse en la direccion opuesta a la que actualmente se encuentra mirando ya que se chocaria consigo misma.
      
                if (Input.GetKey(KeyCode.W) && !_lookingDirection.Equals(Vector3.back))
                {
                    _lookingDirection = Vector3.forward;
                }
                else if (Input.GetKey(KeyCode.S) && !_lookingDirection.Equals(Vector3.forward))
                {
                    _lookingDirection = Vector3.back;
                }
                else if (Input.GetKey(KeyCode.D) && !_lookingDirection.Equals(Vector3.left))
                {
                    _lookingDirection = Vector3.right;
                }
                else if (Input.GetKey(KeyCode.A) && !_lookingDirection.Equals(Vector3.right))
                {
                    _lookingDirection = Vector3.left;
                }
            
            
                Move(this.transform.position + _lookingDirection * movingDistance); 
                
                _timeToNextTick = tickRate; //Reiniciamos el contador para la siguiente vez
            }
        }        
    }

    private void Move(Vector3 newPos)
    {
        var previousPosition = this.transform.position;

        if (newPos.x > maxX)
        {
            newPos = new Vector3(minX,newPos.y,newPos.z);
        }
        else if (newPos.x < minX)
        {
            newPos = new Vector3(maxX,newPos.y,newPos.z);
        }
        else if (newPos.z > maxZ)
        {
            newPos = new Vector3(newPos.x,newPos.y,minZ);
        }
        else if (newPos.z < minZ)
        {
            newPos = new Vector3(newPos.x,newPos.y,maxZ);
        }
        
        this.transform.position = newPos;

        if (_next != null)
        {
            _next.Move(previousPosition);
        }
        else if(shouldSpawn)
        {
            shouldSpawn = false;
            //Soy el ultimo nodo y me toca spawnear otro nodo en la posición que acabo de abandonar
            this._next = Instantiate(snakeNodePrefab, previousPosition, Quaternion.identity).GetComponent<SnakeNode>();
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fruit"))
        {
            fruitEaten.Invoke();
            
            //Movemos la frutita a una posición diferente
            other.transform.position = GenerateRandomFruitPosition();
            
            //Aceleramos el juego cada vez que se agarra una fruta
            tickRate *= 0.9f;
            
            //Hay que spawnear un nodo extra de la viborita
            Spawn();
        }
        else
        {
            //Chocamos contra algo que NO es una fruta, perdimos :c
            Debug.Log(other.name);
            SceneManager.LoadScene("StartupScene");

        }
    }

    private void Spawn()
    {
        if (_next != null)
        {
            //No soy el ultimo nodo, asi que le paso el mensaje al siguiente
            _next.Spawn();
        }
        else
        {
            //Soy el ultimo nodo, cuando me mueva deberia spawnear un nodo mas;
            shouldSpawn = true;
        }
    }
}
