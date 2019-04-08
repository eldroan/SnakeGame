using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreController : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private int scorePoints;
    private int _score;
    
    public void OnScorePoints()
    {
        _score += scorePoints;
        scoreText.text = "Puntaje: " + _score;
    }
}
