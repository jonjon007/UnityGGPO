using SharedGame;
using System.Collections.Generic;
using UnityGGPO;

namespace SimpPlatformer {

    public class SpGameManager : GameManager {

        public override void StartLocalGame() {
            StartGame(new LocalRunner(new SpGame(2)));
        }

        public override void StartGGPOGame(IPerfUpdate perfPanel, IList<Connections> connections, int playerIndex) {
            var game = new GGPORunner("simpplatformer", new SpGame(connections.Count), perfPanel);
            game.Init(connections, playerIndex);
            StartGame(game);
        }
    }
}