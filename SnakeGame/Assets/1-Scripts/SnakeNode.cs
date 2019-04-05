using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
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
    private float _timeToNextTick;
    private static bool _firstNode = true;

    [Header("Valores del nodo")]
    private bool _isHead;
    private SnakeNode _next = null;
    private Vector3 _lookingDirection;
    private bool shouldSpawn;

    private void Awake()
    {
        if (_firstNode)
        {
            _isHead = true; //Si soy el primer nodo en ser creado me seteo como la cabeza de la viborita
            _firstNode = false;
            var myRigidbody = this.gameObject.AddComponent<Rigidbody>();
            myRigidbody.isKinematic = true;
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
        this.transform.position = newPos;

        if (_next != null)
        {
            _next.Move(previousPosition);
        }
    }
}
