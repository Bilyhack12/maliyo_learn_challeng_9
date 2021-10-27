using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const int COIN_SCORE_AMOUNT = 5;
    public static GameManager Instance {set; get;}
    public bool isDead = false;
    private bool isGameStarted = false;
    private PlayerMotor motor;
    private AudioSource audio;

    public Text scoreText, coinText, modifierText;
    public float score, coinScore, modifierScore;
    private int lastScore = 0;

    public Animator deathMenuAnim;
    public AudioClip deathClip, collectClip;
    public Text deadScoreText, deadCoinText;
    private void Awake(){
        Instance = this;
        audio = GetComponent<AudioSource>();
        motor = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMotor>();
        modifierScore = 1;
        modifierText.text = "x"+modifierScore.ToString("0.0");
        scoreText.text = score.ToString("0");
        coinText.text = coinScore.ToString("0");
    }

    private void Update(){
        if(MobileInput.Instance.Tap && !isGameStarted){
            isGameStarted = true;
            motor.StartRunning();
        }

        if(isGameStarted && !isDead){
            score += (Time.deltaTime * modifierScore);
            if(lastScore != (int)score){
                lastScore = (int)score;
                scoreText.text = score.ToString("0");
            }
        }
    }

    public void GetCoin(){
        coinScore += COIN_SCORE_AMOUNT;
        coinText.text = coinScore.ToString("0");
        audio.clip = collectClip;
        audio.Play();
    }

    public void UpdateModifier(float modifierAmount){
        modifierScore = 1.0f + modifierAmount;
        modifierText.text = "x"+modifierScore.ToString("0.0");
    }

    public void OnPlayButton(){
        SceneManager.LoadScene("Game");
    }

    public void OnDeath(){
        audio.clip = deathClip;
        audio.Play();
        isDead = true;
        deadScoreText.text = score.ToString("0");
        deadCoinText.text = coinScore.ToString("0");
        deathMenuAnim.SetTrigger("Dead");
    }
}
