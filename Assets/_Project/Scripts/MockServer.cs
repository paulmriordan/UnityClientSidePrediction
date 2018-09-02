﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MockServer : MonoBehaviour
{
    public event Action<StateMessage> NewServerMessage;

    [SerializeField] Client m_client = null;
    [SerializeField] PaddleController m_serverPaddle = null;
    [SerializeField] BallLauncher m_ball = null;
    [SerializeField] Rigidbody[] m_syncedRigidbodies = null;
    [SerializeField] GameObject m_sceneRoot = null;

    private Queue<InputMessage> m_receivedMessages = new Queue<InputMessage>();

    void Awake ()
    {
        m_client.NewClientMessage += ReceiveClientMessage;
        NewServerMessage += m_client.ReceiveServerMessage;
    }

    public void ReceiveClientMessage(InputMessage input_msg)
    {
        m_receivedMessages.Enqueue(input_msg);
    }

    void FixedUpdate ()
    {
        m_client.SetSceneActive(false);
        m_sceneRoot.SetActive(true);

        UpdateServer(Time.fixedDeltaTime);

        m_client.SetSceneActive(true);
        m_sceneRoot.SetActive(false);
    }

    private void UpdateServer(float dt)
    {
        while (m_receivedMessages.Count > 0)
        {
            if (m_ball.OutOfPlay)
            {
                m_ball.Launch();
            }

            InputMessage input_msg = m_receivedMessages.Dequeue();
            m_serverPaddle.ApplyInput(input_msg.input);

            Physics.Simulate(dt);

            SendStateToClient();
        }
    }

    private void SendStateToClient()
    {
        StateMessage stateMessage;
        stateMessage.rigidbody_states = new RigidbodyState[m_syncedRigidbodies.Length];
        for (int i_rb = 0; i_rb < m_syncedRigidbodies.Length; i_rb++)
        {
            stateMessage.rigidbody_states[i_rb].position = m_syncedRigidbodies[i_rb].position;
            stateMessage.rigidbody_states[i_rb].rotation = m_syncedRigidbodies[i_rb].rotation;
            stateMessage.rigidbody_states[i_rb].velocity = m_syncedRigidbodies[i_rb].velocity;
            stateMessage.rigidbody_states[i_rb].angular_velocity = m_syncedRigidbodies[i_rb].angularVelocity;
        }

        if (NewServerMessage != null)
        {
            NewServerMessage(stateMessage);
        }
    }
}