using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private int scoreIncrement;
    private int _score;

    public void Score()
    {
        _score += scoreIncrement;
        scoreText.text = "Score: " + _score;
    }
}
