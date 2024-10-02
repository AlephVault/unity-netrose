using AlephVault.Unity.Meetgard.Authoring.Behaviours.Client;
using AlephVault.Unity.NetRose.Samples.Client.Protocols;
using UnityEngine;


namespace AlephVault.Unity.NetRose
{
    namespace Samples
    {
        namespace Client
        {
            [RequireComponent(typeof(NetworkClient))]
            [RequireComponent(typeof(ClientMovementProtocolClientSide))]
            public class SampleClientHandler : MonoBehaviour
            {
                [SerializeField]
                private KeyCode modeSwitch = KeyCode.Q;

                private bool mode2 = false;

                [SerializeField]
                private KeyCode mode1StartKey = KeyCode.A;

                [SerializeField]
                private KeyCode mode1StopKey = KeyCode.S;

                [SerializeField]
                private KeyCode mode1Left = KeyCode.LeftArrow;

                [SerializeField]
                private KeyCode mode1Up = KeyCode.UpArrow;

                [SerializeField]
                private KeyCode mode1Down = KeyCode.DownArrow;

                [SerializeField]
                private KeyCode mode1Right = KeyCode.RightArrow;

                [SerializeField]
                private KeyCode mode2StartKey = KeyCode.D;

                [SerializeField]
                private KeyCode mode2StopKey = KeyCode.F;

                [SerializeField]
                private KeyCode mode2Left = KeyCode.G;

                [SerializeField]
                private KeyCode mode2Up = KeyCode.Y;

                [SerializeField]
                private KeyCode mode2Down = KeyCode.H;

                [SerializeField]
                private KeyCode mode2Right = KeyCode.J;

                private NetworkClient client;
                private ClientMovementProtocolClientSide protocol;

                private void Awake()
                {
                    client = GetComponent<NetworkClient>();
                    protocol = GetComponent<ClientMovementProtocolClientSide>();
                }

                void Update()
                {
                    if (Input.GetKeyDown(modeSwitch))
                    {
                        mode2 = !mode2;
                        Debug.Log($"Switching to mode: {(mode2 ? 2 : 1)}");
                    }

                    if (Input.GetKeyDown(mode2 ? mode2StartKey : mode1StartKey) && !client.IsRunning && !client.IsConnected)
                    {
                        Debug.Log("Sample Server::Starting...");
                        // In your computer: map 127.0.0.1 -> test.alephvault.com.
                        // This serves in particular for when using SSL=true.
                        client.Connect("test.alephvault.com", 9999);
                        Debug.Log("Sample Server::Started.");
                    }

                    if (client.IsRunning && client.IsConnected)
                    {
                        if (Input.GetKeyDown(mode2 ? mode2StopKey : mode1StopKey))
                        {
                            Debug.Log("Client::Disconnecting...");
                            client.Close();
                            Debug.Log("Client::Disconnected.");
                        }
                        else if (Input.GetKey(mode2 ? mode2Left : mode1Left))
                        {
                            Debug.Log("Client::Moving < ...");
                            protocol.WalkLeft();
                            Debug.Log("Client::Moved <.");
                        }
                        else if (Input.GetKey(mode2 ? mode2Up : mode1Up))
                        {
                            Debug.Log("Client::Moving ^ ...");
                            protocol.WalkUp();
                            Debug.Log("Client::Moved ^.");
                        }
                        else if (Input.GetKey(mode2 ? mode2Down : mode1Down))
                        {
                            Debug.Log("Client::Moving v ...");
                            protocol.WalkDown();
                            Debug.Log("Client::Moved v.");
                        }
                        else if (Input.GetKey(mode2 ? mode2Right : mode1Right))
                        {
                            Debug.Log("Client::Moving > ...");
                            protocol.WalkRight();
                            Debug.Log("Client::Moved >.");
                        }
                    }
                }
            }
        }
    }
}
