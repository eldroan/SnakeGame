using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class SnakeController : MonoBehaviour
{
    [Header("Valores para configurar el juego")]
    [SerializeField] private Transform floorTransform;//Esto solo se usa para sacar el tamaño del tablero
    [SerializeField] private float tickRate;//cada cuantos segundos se realiza un tickde movimiento para la serpiente;
    [SerializeField] private float moveDistance;
    [SerializeField] private UnityEvent fruitEaten;
    [Header("Valores para instanciar elementos")]
    [SerializeField] private GameObject cubePrefab; //Este prefab es el único que se utilizará, se le va a cambiar el material dependiendo que sea
    [SerializeField] private Material snakeMaterial;
    [SerializeField] private Material fruitMaterial;

    private float _floorMagnitudeX, _floorMagnitudeZ;
    private List<Transform> _nodes;
    private float _timeToNextTick;
    private Vector3 _lookingDirection;
    private bool _shouldSpawn;
    private GameObject _fruit = null;
    

    // Start is called before the first frame update
    void Awake()
    {
        //Los asserts sirven para controlar que hemos seteado los valores que necesitamos desde el editor, no son necesarios para que funcione el juego pero sirven para debuggear.
        
        Assert.IsNotNull(snakeMaterial, "snakeMaterial no puede ser null");
        Assert.IsNotNull(fruitMaterial, "fruitMaterial no puede ser null");
        Assert.IsNotNull(cubePrefab, "El prefab de cubo no puede ser null ya que se utiliza para spawnear nodos y frutas");
        Assert.IsNotNull(floorTransform, "Floor transform no puede ser nulo porque se utiliza para saber el tamaño del tablero");
        Assert.AreNotEqual(0,tickRate,"TickRate no puede valer 0"); 
        Assert.AreNotEqual(0,moveDistance,"moveDistance no puede valer 0"); 
        
        _nodes = new List<Transform>();
        _nodes.Add(this.transform);

        var localScale = floorTransform.localScale;
        _floorMagnitudeX = localScale.x / 2f;
        _floorMagnitudeZ = localScale.z / 2f;
        
        _timeToNextTick = tickRate;
        _lookingDirection = Vector3.right;
        
        SpawnFruit();
    }

    // Update is called once per frame
    void Update()
    {
        _timeToNextTick = _timeToNextTick - Time.deltaTime;

        if (_timeToNextTick < 0)
        {
            _timeToNextTick = tickRate;
            
            var horizontal = Input.GetAxisRaw("Horizontal"); 
            var vertical = Input.GetAxisRaw("Vertical");

            if (vertical == 1 && !_lookingDirection.Equals(Vector3.back))
            {
                _lookingDirection = Vector3.forward;
                this.transform.rotation = Quaternion.Euler(0,270,0);
            }
            else if (vertical == -1 && !_lookingDirection.Equals(Vector3.forward))
            {
                _lookingDirection = Vector3.back;
                this.transform.rotation = Quaternion.Euler(0,90,0);
            }
            else if (horizontal == 1 && !_lookingDirection.Equals(Vector3.left))
            {
                _lookingDirection = Vector3.right;
                this.transform.rotation = Quaternion.identity;
            }
            else if (horizontal == -1 && !_lookingDirection.Equals(Vector3.right))
            {
                _lookingDirection = Vector3.left;
                this.transform.rotation = Quaternion.Euler(0,180,0);
            }

            Move();

        }
    }

    private void Move()
    {
        var nextPosition = this.transform.position + _lookingDirection * moveDistance;

        if (nextPosition.x > _floorMagnitudeX)
        {
            nextPosition = new Vector3(_floorMagnitudeX * -1,nextPosition.y,nextPosition.z);
        }
        else if (nextPosition.x < _floorMagnitudeX * -1)
        {
            nextPosition = new Vector3(_floorMagnitudeX ,nextPosition.y,nextPosition.z);
        }
        else if (nextPosition.z > _floorMagnitudeZ)
        {
            nextPosition = new Vector3(nextPosition.x,nextPosition.y,_floorMagnitudeZ * -1);
        }
        else if (nextPosition.z < _floorMagnitudeZ * -1)
        {
            nextPosition = new Vector3(nextPosition.x,nextPosition.y,_floorMagnitudeZ );

        }

        //Muevo todos los nodos de la lista
        foreach (var node in _nodes)
        {
            var currentPos = node.transform.position;
            node.transform.position = nextPosition;
            nextPosition = currentPos;
        }
        
        //Si hay que spawnear uno mas instancio un cubo, le seteo el material de snake y lo agrego a la lista;
        if (_shouldSpawn)
        {
            _shouldSpawn = false;
            var newNode = Instantiate(cubePrefab, nextPosition, Quaternion.identity);
            _nodes.Add(newNode.transform);
            newNode.GetComponent<MeshRenderer>().material = snakeMaterial;
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fruit"))
        {
            _shouldSpawn = true; //Hay que spawnear un nuevo cubo
            
            tickRate = tickRate * 0.95f; // Cada vez que agarramos una fruta aceleramos el juego;

            //Chocamos con una frutita, la movemos a otro lado
            _fruit.transform.position = RandomFruitPosition();
            
            fruitEaten.Invoke(); //Invocamos el evento fruitEaten para que los interesados puedan reaccionar acordemente
        }
        else
        {
            //Chocamos con otra cosa y perdimos :c
            SceneManager.LoadScene("TitleScene");
        }
    }

    private void SpawnFruit()
    {
        //La primera vez la creamos y nos guardamos la referencia
        _fruit = Instantiate(cubePrefab, RandomFruitPosition(), Quaternion.identity);
        _fruit.tag = "Fruit"; //Si no seteamos este tag nunca sabrá que choco contra una fruta
        _fruit.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
        _fruit.GetComponent<MeshRenderer>().material = fruitMaterial;
        
    }

    private Vector3 RandomFruitPosition()
    {
        var headPos = this.transform.position;
        Vector3 fruitPos;

        do
        {
            fruitPos = new Vector3(Random.Range(_floorMagnitudeX * -1, _floorMagnitudeX), -0.25f, Random.Range(_floorMagnitudeZ * -1, _floorMagnitudeZ));
        } while (fruitPos.Equals(headPos)); // Asi garantizamos que la fruta nunca spawnee en la posicion de la cabeza

        return fruitPos;
    }
}
