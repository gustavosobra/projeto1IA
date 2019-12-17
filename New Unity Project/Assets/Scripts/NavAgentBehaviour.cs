using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using LibGameAI.FSMs;
using System;
using URandom = UnityEngine.Random;

public class NavAgentBehaviour : MonoBehaviour
{
    // Reference game object for the goal
    private GameObject goalObject;

    // Current goal of navigation agent
    [SerializeField] private Transform goal = null;

    // Variables for hunger and tired
    [SerializeField] private float hungerMeter;
    [SerializeField] private float tiredMeter;

    private float i;
    private float timeStunned;

    // Variables to check if the agent is resting or eating
    private bool isEating;
    private bool isResting;
    private bool secondRadius;
    private bool thirdRadius;
    private bool arrivedRestaurante;
    private bool arrivedZonaVerde;


    private StateMachine stateMachine;

    // Reference to the NavMeshAgent component
    private NavMeshAgent agent;

    // Start is called before the first frame update
    private void Start()
    {
        // Gives the agent a random speed to walk
        GetComponent<NavMeshAgent>().speed = URandom.Range(1f, 2f);
        // Gives the agent a random value for the hunger and tired level
        hungerMeter = URandom.Range(0f, 200f);
        tiredMeter = URandom.Range(0f, 200f);
        i = URandom.Range(0f, 1f);

        isEating = false;
        isResting = false;
        secondRadius = false;
        thirdRadius = false;
        arrivedRestaurante = false;
        arrivedZonaVerde = false;
        
        // Get reference to the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();
        // Set initial agent goal

        //Creation of my FSM
        State watchingConcertState = new State(
            "WatchingConcert",
            () => Debug.Log("Entering WatchingConcert"),
            chooseStage,
            () => Debug.Log("Exiting WatchingConcert"));

        State eatState = new State(
            "Eating",
            () => Debug.Log("Entering Eat"),
            getFood,
            () => Debug.Log("Exiting Eat"));

        State restState = new State(
            "Resting",
            () => Debug.Log("Entering Rest"),
            goRest,
            () => Debug.Log("Exiting Rest"));

        State panicState = new State(
            "Panic",
            () => Debug.Log("Entering Panic"),
            Run,
            null);

        watchingConcertState.AddTransition(new Transition(
            () => hungerMeter <= 5f,
            null,
            eatState));

        watchingConcertState.AddTransition(new Transition(
            () => tiredMeter <= 5f,
            null,
            restState));

        watchingConcertState.AddTransition(new Transition(
            () => secondRadius || thirdRadius,
            null,
            panicState));

        eatState.AddTransition(new Transition(
            () => hungerMeter >= 195f && tiredMeter > 5f,
            null,
            watchingConcertState));

        eatState.AddTransition(new Transition(
            () => hungerMeter >= 195f && tiredMeter <= 5f,
            null,
            restState));

        eatState.AddTransition(new Transition(
            () => secondRadius || thirdRadius,
            null,
            panicState));

        restState.AddTransition(new Transition(
            () => hungerMeter > 5f && tiredMeter >= 195f,
            null,
            watchingConcertState));

        restState.AddTransition(new Transition(
            () => hungerMeter <= 5f && tiredMeter >= 195f,
            null,
            eatState));

        restState.AddTransition(new Transition(
            () => secondRadius || thirdRadius,
            null,
            panicState));

        stateMachine = new StateMachine(watchingConcertState);
    }

    private void Update()
    {
        Action actions = stateMachine.Update();
        actions?.Invoke();

        // Hunger and Tired meters go down over time
        if (!isEating)
            hungerLoss();

        if(!isResting)
            tiredLoss();
    }

    private void hungerLoss()
    {
        hungerMeter -= 1f * Time.deltaTime;

        if (hungerMeter <= 5f && isResting == false)
            isEating = true;
    }

    private void tiredLoss()
    {
        tiredMeter -= 1f * Time.deltaTime;

        if (tiredMeter <= 5f && isEating == false)
            isResting = true;
    }

    private void chooseStage()
    {
        //Debug.Log(i);
        if(i <= 0.5f)
        {
            goalObject = GameObject.Find("Palco1");
            goal = goalObject.transform;
        }
        else
        {
            goalObject = GameObject.Find("Palco2");
            goal = goalObject.transform;
        }

        agent.destination = goal.position;
    }

    private void getFood()
    {
        goalObject = GameObject.Find("Restaurante");
        goal = goalObject.transform;

        agent.destination = goal.position;

        if(arrivedRestaurante)
            hungerMeter += 10f * Time.deltaTime;

        if (hungerMeter >= 195f)
        {
            isEating = false;
            arrivedRestaurante = false;
        }
    }

    private void goRest()
    {
        if (i <= 0.5f)
        {
            goalObject = GameObject.Find("Zona1");
            goal = goalObject.transform;
        }
        else
        {
            goalObject = GameObject.Find("Zona2");
            goal = goalObject.transform;
        }

        agent.destination = goal.position;

        if(arrivedZonaVerde)
            tiredMeter += 10f * Time.deltaTime;

        if (tiredMeter >= 195f)
        {
            isResting = false;
            arrivedZonaVerde = false;
        }
    }

    private void Run()
    {
        goalObject = GameObject.Find("Saida");
        goal = goalObject.transform;

        agent.destination = goal.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("2radius"))
        {
            secondRadius = true;
            if(secondRadius && thirdRadius)
                GetComponent<NavMeshAgent>().speed /= 2f;
        }

        if (other.CompareTag("3radius"))
        {
            thirdRadius = true;
            if (!secondRadius && thirdRadius)
                GetComponent<NavMeshAgent>().speed *= 2f;
        }

        if (other.CompareTag("Restaurante"))
            arrivedRestaurante = true;

        if (other.CompareTag("ZonaVerde"))
            arrivedZonaVerde = true;
    }
}
