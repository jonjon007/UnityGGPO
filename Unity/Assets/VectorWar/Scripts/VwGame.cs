using SharedGame;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;

namespace VectorWar {

    using static VWConstants;

    public static class VWConstants {
        public const int MAX_SHIPS = 4;
        public const int MAX_PLAYERS = 64;

        public const int INPUT_THRUST = (1 << 0);
        public const int INPUT_BREAK = (1 << 1);
        public const int INPUT_ROTATE_LEFT = (1 << 2);
        public const int INPUT_ROTATE_RIGHT = (1 << 3);
        public const int INPUT_FIRE = (1 << 4);
        public const int INPUT_BOMB = (1 << 5);
        public const int INPUT_UP = (1 << 6);
        public const int INPUT_DOWN = (1 << 7);
        public const int INPUT_LEFT = (1 << 8);
        public const int INPUT_RIGHT = (1 << 9);
        public const int INPUT_UP_THIS_FRAME = (1 << 106);
        public const int INPUT_DOWN_THIS_FRAME = (1 << 107);
        public const int INPUT_LEFT_THIS_FRAME = (1 << 108);
        public const int INPUT_RIGHT_THIS_FRAME = (1 << 109);
        public const int MAX_BULLETS = 30;

        public const float PI = 3.1415926f;
        public const int STARTING_HEALTH = 100;
        public const float ROTATE_INCREMENT = 3f;
        public const float SHIP_RADIUS = 15f;
        public const float SHIP_THRUST = 0.06f;
        public const float SHIP_MAX_THRUST = 4.0f;
        public const float SHIP_BREAK_SPEED = 0.6f;
        public const float BULLET_SPEED = 5f;
        public const int BULLET_COOLDOWN = 8;
        public const int BULLET_DAMAGE = 10;

        public const string P1_UP_KEY = "w";
        public const string P1_DOWN_KEY = "s";
        public const string P1_LEFT_KEY = "a";
        public const string P1_RIGHT_KEY = "d";
        public const string P2_UP_KEY = "up";
        public const string P2_DOWN_KEY = "down";
        public const string P2_RIGHT_KEY = "right";
        public const string P2_LEFT_KEY = "left";
        public const string P1_FIRE_KEY = "space";
        public const string P2_FIRE_KEY = "enter";
    }

    [Serializable]
    public struct Bullet {
        public bool active;
        public Vector2 position;
        public Vector2 velocity;

        public void Serialize(BinaryWriter bw) {
            bw.Write(active);
            bw.Write(position.x);
            bw.Write(position.y);
            bw.Write(velocity.x);
            bw.Write(velocity.y);
        }

        public void Deserialize(BinaryReader br) {
            active = br.ReadBoolean();
            position.x = br.ReadSingle();
            position.y = br.ReadSingle();
            velocity.x = br.ReadSingle();
            velocity.y = br.ReadSingle();
        }
    };

    [Serializable]
    public class Ship {
        public Vector2 position;
        public Vector2 velocity;
        public float radius;
        public float heading;
        public int health;
        public int cooldown;
        public int score;
        public Bullet[] bullets = new Bullet[MAX_BULLETS];

        public void Serialize(BinaryWriter bw) {
            bw.Write(position.x);
            bw.Write(position.y);
            bw.Write(velocity.x);
            bw.Write(velocity.y);
            bw.Write(radius);
            bw.Write(heading);
            bw.Write(health);
            bw.Write(cooldown);
            bw.Write(score);
            for (int i = 0; i < MAX_BULLETS; ++i) {
                bullets[i].Serialize(bw);
            }
        }

        public void Deserialize(BinaryReader br) {
            position.x = br.ReadSingle();
            position.y = br.ReadSingle();
            velocity.x = br.ReadSingle();
            velocity.y = br.ReadSingle();
            radius = br.ReadSingle();
            heading = br.ReadSingle();
            health = br.ReadInt32();
            cooldown = br.ReadInt32();
            score = br.ReadInt32();
            for (int i = 0; i < MAX_BULLETS; ++i) {
                bullets[i].Deserialize(br);
            }
        }

        // @LOOK Not hashing bullets.
        public override int GetHashCode() {
            int hashCode = 1858597544;
            hashCode = hashCode * -1521134295 + position.GetHashCode();
            hashCode = hashCode * -1521134295 + velocity.GetHashCode();
            hashCode = hashCode * -1521134295 + radius.GetHashCode();
            hashCode = hashCode * -1521134295 + heading.GetHashCode();
            hashCode = hashCode * -1521134295 + health.GetHashCode();
            hashCode = hashCode * -1521134295 + cooldown.GetHashCode();
            hashCode = hashCode * -1521134295 + score.GetHashCode();
            return hashCode;
        }
    }

    [Serializable]
    public class Box {
        public int instanceId;
        public Vector3Int position;

        public void Serialize(BinaryWriter bw) {
            bw.Write(instanceId);
            bw.Write(position.x);
            bw.Write(position.y);
            bw.Write(position.z);
        }

        public void Deserialize(BinaryReader br) {
            instanceId = br.ReadInt32();
            position.x = br.ReadInt32();
            position.y = br.ReadInt32();
            position.z = br.ReadInt32();
        }

        public override int GetHashCode() {
            int hashCode = 1858537542;
            hashCode = hashCode * -1521134295 + instanceId.GetHashCode();
            hashCode = hashCode * -1521134295 + position.GetHashCode();
            return hashCode;
        }
    }

    [Serializable]
    public struct VwGame : IGame {
        Dictionary<int, GameObject> objectIDMap;
        public int Framenumber { get; private set; }

        public int Checksum => GetHashCode();

        public Ship[] _ships;
        public Box[] _boxes;

        public static Rect _bounds = new Rect(0, 0, 640, 480);

        public void Serialize(BinaryWriter bw) {
            bw.Write(Framenumber);
            bw.Write(_ships.Length);
            for (int i = 0; i < _ships.Length; ++i) {
                _ships[i].Serialize(bw);
            }
            bw.Write(_boxes.Length);
            for (int i = 0; i < _boxes.Length; ++i) {
                _boxes[i].Serialize(bw);
            }
        }

        public void Deserialize(BinaryReader br) {
            Framenumber = br.ReadInt32();
            int ship_length = br.ReadInt32();
            if (ship_length != _ships.Length) {
                _ships = new Ship[ship_length];
            }
            for (int i = 0; i < _ships.Length; ++i) {
                _ships[i].Deserialize(br);
            }

            int box_length = br.ReadInt32();
            if (box_length != _boxes.Length) {
                _boxes = new Box[box_length];
            }
            for (int i = 0; i < _boxes.Length; ++i) {
                _boxes[i].Deserialize(br);
            }
        }

        /* Gets called on shutdown */
        public void CleanUp(){
            //Destroy all gameobjects
            foreach(KeyValuePair<int, GameObject> kvp in objectIDMap){
                GameObject g = kvp.Value;
                GameObject.Destroy(g);
            }
            objectIDMap.Clear();
        }

        public NativeArray<byte> ToBytes() {
            using (var memoryStream = new MemoryStream()) {
                using (var writer = new BinaryWriter(memoryStream)) {
                    Serialize(writer);
                }
                return new NativeArray<byte>(memoryStream.ToArray(), Allocator.Persistent);
            }
        }

        public void FromBytes(NativeArray<byte> bytes) {
            using (var memoryStream = new MemoryStream(bytes.ToArray())) {
                using (var reader = new BinaryReader(memoryStream)) {
                    Deserialize(reader);
                }
            }
        }

        /* Determines direction number based on movement inputs*/
        private int GetDirNumber(int v, int h){
            if(h > 0){
                if(v > 0)
                    return 9; //Up-right
                if (v < 0)
                    return 3; //Down-right
                return 6; //Right
            }
            if(h < 0){
                if(v > 0)
                    return 7; //Up-left
                if (v < 0)
                    return 1; //Down-left
                return 4; //Left
            }
            if(v > 0)
                return 8; //Up
            if (v < 0)
                return 2; //Down
            return 5; //Neutral
        }

        private GameObject GetObjectFromID(int id){
            GameObject result = null;
            objectIDMap.TryGetValue(id, out result);
            if(result == null)
                Debug.Log("GO not found.");
            return result;
        }

        private static float DegToRad(float deg) {
            return PI * deg / 180;
        }

        private static float Distance(Vector2 lhs, Vector2 rhs) {
            float x = rhs.x - lhs.x;
            float y = rhs.y - lhs.y;
            return Mathf.Sqrt(x * x + y * y);
        }

        private bool CheckInput(long currentInputs, int input){
            return (currentInputs & input) != 0;
        }

        /* Takes a direction number (1-9) and returns a Vector2 representing that direction. */
        private Vector2Int GetVectorFromDirNumber(int dirNum){
            Vector2Int result = new Vector2Int(0,0);
            // TODO: Determine what's faster: math or checking with exact values?
            // Positive X (3,6,9)
            if(dirNum % 3 == 0){
                result.x = 1;
            }
            // Positive Y (7,8,9)
            if((dirNum-1) / 3 == 2){
                result.y = 1;
            }
            // Negative X (1,4,7)
            if(dirNum % 3 == 1){
                result.x = -1;
            }
            // Negative Y (1,2,3)
            if((dirNum-1) / 3 == 0){
                result.y = -1;
            }

            return result;
        }

        /*
         * InitGameState --
         *
         * Initialize our game state.
         */

        public VwGame(int num_players) {
            var w = _bounds.xMax - _bounds.xMin;
            var h = _bounds.yMax - _bounds.yMin;
            var r = h / 4;
            Framenumber = 0;
            objectIDMap = new Dictionary<int, GameObject>();
            // Spawn ships
            _ships = new Ship[num_players];
            for (int i = 0; i < _ships.Length; i++) {
                _ships[i] = new Ship();
                int heading = i * 360 / num_players;
                float cost, sint, theta;

                theta = (float)heading * PI / 180;
                cost = Mathf.Cos(theta);
                sint = Mathf.Sin(theta);

                _ships[i].position.x = (w / 2) + r * cost;
                _ships[i].position.y = (h / 2) + r * sint;
                _ships[i].heading = (heading + 180) % 360;
                _ships[i].health = STARTING_HEALTH;
                _ships[i].radius = SHIP_RADIUS;
            }
            // Spawn boxes
            _boxes = new Box[num_players];
            for (int i = 0; i < _boxes.Length; i++) {
                createBox(i);
            }
        }

        private void createBox(int player){
            _boxes[player] = new Box();
            // Get new position and assign it
            Vector3Int pos = new Vector3Int(player+320, 228+player, -9);
            _boxes[player].position = pos;

            // Create game object and assign instance id
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = pos;
            int id =cube.GetInstanceID();
            _boxes[player].instanceId = id;

            // Add object and id to map
            objectIDMap.Add(id, cube);
        }

        public void GetShipAI(int i, out float heading, out float thrust, out int fire) {
            heading = (_ships[i].heading + 5) % 360;
            thrust = 0;
            fire = 0;
        }

        public void ParseBoxInputs(long inputs, int i, out int dir, out int dirThisFrame) {
            dir = 0; //TODO: Update
            int v = 0;
            int h = 0;

            // Vertical
            if(CheckInput(inputs, INPUT_UP_THIS_FRAME))
                v = 1;
            else if(CheckInput(inputs, INPUT_DOWN_THIS_FRAME))
                v = -1;

            // Horizontal
            if(CheckInput(inputs, INPUT_RIGHT_THIS_FRAME))
                h = 1;
            else if(CheckInput(inputs, INPUT_LEFT_THIS_FRAME))
                h = -1;

            dirThisFrame = GetDirNumber(v, h);
        }

        public void ParseShipInputs(long inputs, int i, out float heading, out float thrust, out int fire) {
            var ship = _ships[i];

            GGPORunner.LogGame($"parsing ship {i} inputs: {inputs}.");

            if ((inputs & INPUT_RIGHT) != 0) {
                heading = (ship.heading - ROTATE_INCREMENT) % 360;
            }
            else if ((inputs & INPUT_LEFT) != 0) {
                heading = (ship.heading + ROTATE_INCREMENT + 360) % 360;
            }
            else {
                heading = ship.heading;
            }

            if ((inputs & INPUT_UP) != 0) {
                thrust = SHIP_THRUST;
            }
            else if ((inputs & INPUT_DOWN) != 0) {
                thrust = -SHIP_THRUST;
            }
            else {
                thrust = 0;
            }
            fire = (int)(inputs & INPUT_FIRE);
        }

        public void MoveBox(int index, int dirThisFrame) {
            var box = _boxes[index];

            if (dirThisFrame > 0) {
                Vector2Int dirVector = GetVectorFromDirNumber(dirThisFrame);
                int dx = dirVector.x;
                int dy = dirVector.y;
                int dz = 0;
                GGPORunner.LogGame("Moving box up.");
                box.position.x = box.position.x + dx;
                box.position.y = box.position.y + dy;
                box.position.z = box.position.z + dz;
                GGPORunner.LogGame($"New box position: {box.position.x},{box.position.y},{box.position.z}");
            }

            // Move physical box
            GameObject boxObj = GetObjectFromID(box.instanceId);
            boxObj.transform.position = box.position;
        }

        public void MoveShip(int index, float heading, float thrust, int fire) {
            var ship = _ships[index];

            GGPORunner.LogGame($"calculation of new ship coordinates: (thrust:{thrust} heading:{heading}).");

            ship.heading = heading;

            if (ship.cooldown == 0) {
                if (fire != 0) {
                    GGPORunner.LogGame("firing bullet.");
                    for (int i = 0; i < ship.bullets.Length; i++) {
                        float dx = Mathf.Cos(DegToRad(ship.heading));
                        float dy = Mathf.Sin(DegToRad(ship.heading));
                        if (!ship.bullets[i].active) {
                            ship.bullets[i].active = true;
                            ship.bullets[i].position.x = ship.position.x + (ship.radius * dx);
                            ship.bullets[i].position.y = ship.position.y + (ship.radius * dy);
                            ship.bullets[i].velocity.x = ship.velocity.x + (BULLET_SPEED * dx);
                            ship.bullets[i].velocity.y = ship.velocity.y + (BULLET_SPEED * dy);
                            ship.cooldown = BULLET_COOLDOWN;
                            break;
                        }
                    }
                }
            }

            if (thrust != 0) {
                float dx = thrust * Mathf.Cos(DegToRad(heading));
                float dy = thrust * Mathf.Sin(DegToRad(heading));

                ship.velocity.x += dx;
                ship.velocity.y += dy;
                float mag = Mathf.Sqrt(ship.velocity.x * ship.velocity.x +
                                 ship.velocity.y * ship.velocity.y);
                if (mag > SHIP_MAX_THRUST) {
                    ship.velocity.x = (ship.velocity.x * SHIP_MAX_THRUST) / mag;
                    ship.velocity.y = (ship.velocity.y * SHIP_MAX_THRUST) / mag;
                }
            }
            GGPORunner.LogGame($"new ship velocity: (dx:{ship.velocity.x} dy:{ship.velocity.y}).");

            ship.position.x += ship.velocity.x;
            ship.position.y += ship.velocity.y;
            GGPORunner.LogGame($"new ship position: (dx:{ship.position.x} dy:{ship.position.y}).");

            if (ship.position.x - ship.radius < _bounds.xMin ||
                ship.position.x + ship.radius > _bounds.xMax) {
                ship.velocity.x *= -1;
                ship.position.x += (ship.velocity.x * 2);
            }
            if (ship.position.y - ship.radius < _bounds.yMin ||
                ship.position.y + ship.radius > _bounds.yMax) {
                ship.velocity.y *= -1;
                ship.position.y += (ship.velocity.y * 2);
            }
            for (int i = 0; i < ship.bullets.Length; i++) {
                if (ship.bullets[i].active) {
                    ship.bullets[i].position.x += ship.bullets[i].velocity.x;
                    ship.bullets[i].position.y += ship.bullets[i].velocity.y;
                    if (ship.bullets[i].position.x < _bounds.xMin ||
                        ship.bullets[i].position.y < _bounds.yMin ||
                        ship.bullets[i].position.x > _bounds.xMax ||
                        ship.bullets[i].position.y > _bounds.yMax) {
                        ship.bullets[i].active = false;
                    }
                    else {
                        for (int j = 0; j < _ships.Length; j++) {
                            var other = _ships[j];
                            if (Distance(ship.bullets[i].position, other.position) < other.radius) {
                                ship.score++;
                                other.health -= BULLET_DAMAGE;
                                ship.bullets[i].active = false;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void LogInfo(string filename) {
            string fp = "";
            fp += "GameState object.\n";
            fp += string.Format("  bounds: {0},{1} x {2},{3}.\n", _bounds.xMin, _bounds.yMin, _bounds.xMax, _bounds.yMax);
            fp += string.Format("  num_ships: {0}.\n", _ships.Length);
            for (int i = 0; i < _ships.Length; i++) {
                var ship = _ships[i];
                fp += string.Format("  ship {0} position:  %.4f, %.4f\n", i, ship.position.x, ship.position.y);
                fp += string.Format("  ship {0} velocity:  %.4f, %.4f\n", i, ship.velocity.x, ship.velocity.y);
                fp += string.Format("  ship {0} radius:    %d.\n", i, ship.radius);
                fp += string.Format("  ship {0} heading:   %d.\n", i, ship.heading);
                fp += string.Format("  ship {0} health:    %d.\n", i, ship.health);
                fp += string.Format("  ship {0} cooldown:  %d.\n", i, ship.cooldown);
                fp += string.Format("  ship {0} score:     {1}.\n", i, ship.score);
                for (int j = 0; j < ship.bullets.Length; j++) {
                    fp += string.Format("  ship {0} bullet {1}: {2} {3} -> {4} {5}.\n", i, j,
                            ship.bullets[j].position.x, ship.bullets[j].position.y,
                            ship.bullets[j].velocity.x, ship.bullets[j].velocity.y);
                }
            }
            File.WriteAllText(filename, fp);
        }

        public void Update(long[] inputs, int disconnect_flags) {
            Framenumber++;
            for (int i = 0; i < _boxes.Length; i++) {
                float thrust, heading;
                int fire;
                int dir, dirThisFrame = 0;

                if ((disconnect_flags & (1 << i)) != 0) {
                    GetShipAI(i, out heading, out thrust, out fire);
                }
                else {
                    ParseBoxInputs(inputs[i], i, out dir, out dirThisFrame);
                    ParseShipInputs(inputs[i], i, out heading, out thrust, out fire);
                }
                MoveShip(i, heading, thrust, fire);
                MoveBox(i, dirThisFrame); //TODO: Implement

                if (_ships[i].cooldown != 0) {
                    _ships[i].cooldown--;
                }
            }
        }

        public long ReadInputs(int id, long lastInputs) {
            long input = 0;

            // string shotKey = id == 0 ? "a" : "b";

            // // Check up input
            // if(UnityEngine.Input.GetKey(
            //     id == 0 ? P1_UP_KEY : P2_UP_KEY
            // )){
            //     input |= INPUT_UP;
            //     if((lastInputs & INPUT_UP) == 0
            //         && (lastInputs & INPUT_UP_THIS_FRAME) == 0){
            //         input |= INPUT_UP_THIS_FRAME;
            //     }
            // }

            // // Check down input
            // if(UnityEngine.Input.GetKey(
            //     id == 0 ? P1_DOWN_KEY : P2_DOWN_KEY
            // )){
            //     input |= INPUT_DOWN;
            //     if((lastInputs & INPUT_DOWN) == 0
            //         && (lastInputs & INPUT_DOWN_THIS_FRAME) == 0){
            //         input |= INPUT_DOWN_THIS_FRAME;
            //     }
            // }

            // // Check left input
            // if(UnityEngine.Input.GetKey(
            //     id == 0 ? P1_LEFT_KEY : P2_LEFT_KEY
            // )){
            //     input |= INPUT_LEFT;
            //     if((lastInputs & INPUT_LEFT) == 0
            //         && (lastInputs & INPUT_LEFT_THIS_FRAME) == 0){
            //         input |= INPUT_LEFT_THIS_FRAME;
            //     }
            // }

            // // Check right input
            // if(UnityEngine.Input.GetKey(
            //     id == 0 ? P1_RIGHT_KEY : P2_RIGHT_KEY
            // )){
            //     input |= INPUT_RIGHT;
            //     if((lastInputs & INPUT_RIGHT) == 0
            //         && (lastInputs & INPUT_RIGHT_THIS_FRAME) == 0){
            //         input |= INPUT_RIGHT_THIS_FRAME;
            //     }
            // }

            // // Check fire input
            // if(UnityEngine.Input.GetKey(
            //     id == 0 ? P1_FIRE_KEY : P2_FIRE_KEY
            // )){
            //     input |= INPUT_FIRE;
            // }

            return input;
        }

        public void FreeBytes(NativeArray<byte> data) {
            if (data.IsCreated) {
                data.Dispose();
            }
        }

        public override int GetHashCode() {
            int hashCode = -1214587014;
            hashCode = hashCode * -1521134295 + Framenumber.GetHashCode();
            foreach (var ship in _ships) {
                hashCode = hashCode * -1521134295 + ship.GetHashCode();
            }
            return hashCode;
        }
    }
}