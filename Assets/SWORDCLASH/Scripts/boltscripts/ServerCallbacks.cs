using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using BoltInternal;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
public class ServerCallbacks : Bolt.GlobalEventListener
{

    public override void SceneLoadLocalDone(string sceneName)
    {
        // randomize a position; may have to convert to camera units??? how get camera ref??? move it after???
        // probably will end up instantiating it, then moving it to start position later, will hardcode for now
        // has weird values to account for sceneName's off (000) camera placement
        var spawnPosition = new Vector3(-1.5f, -5.8f, 0);

        
       // instantiate cube; all BoltPrefabs are accessed through a static class
       BoltEntity playerOne = BoltNetwork.Instantiate(BoltPrefabs.TentacleTipScene, spawnPosition, Quaternion.identity);
        playerOne.TakeControl();

        // NOw spawn in GameLogicController which will spawn in food???
        // BoltEntity tentacleFood = BoltNetwork.Attach();

        BoltEntity SCGameLogic = BoltNetwork.Instantiate(BoltPrefabs.SCGameWorld);
        // TakeControl only needed if controller???
        //SCGameLogic.TakeControl();
    }


    public override void SceneLoadRemoteDone(BoltConnection connection)
    {
        var spawnPosition = new Vector3(-1.5f, 1.0f, 0);

        //// instantiate cube; all BoltPrefabs are accessed through a static class
        BoltEntity playerTwo = BoltNetwork.Instantiate(BoltPrefabs.TentacleTipScene, spawnPosition, Quaternion.identity);
        playerTwo.tag = "TentacleTipP2";
        playerTwo.AssignControl(connection);

        
    }

    public override void Connected(BoltConnection connection)
    {
        // In Bolt you use EventName.Create(); to create a new event, you then assign the properties you want and call eventObject.Send();
        var log = LogEvent.Create();
        log.Message = string.Format("{0} connected", connection.RemoteEndPoint);
        log.Send();
    }

    public override void Disconnected(BoltConnection connection)
    {
        var log = LogEvent.Create();
        log.Message = string.Format("{0} disconnected", connection.RemoteEndPoint);
        log.Send();
    }




}