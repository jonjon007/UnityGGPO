using UnityEngine.InputSystem;

namespace SepM.DataTypes{
    public class PlayerControl
    {
        InputDevice device = null; int player; string input; string mapping;
        public PlayerControl(InputDevice d, int p, string i, string m){
            device = d; player = p; input = i; mapping = m;
        }
        public InputDevice getDevice(){ return device; }
        public int getPlayer(){ return player; }
        public string getInput(){ return input; }
        public string getMapping(){ return mapping; }

        /* Compares another PlayerControl */
        public override bool Equals(object obj)
        {
            PlayerControl other = obj as PlayerControl;
            
            return this.device != null && other.device != null
                    && this.device.name == other.device.name
                    && this.player == other.getPlayer()
                    && this.input == other.getInput()
                    && this.mapping == other.getMapping();
        }

        public override int GetHashCode() {
            var hashCode = 352033287;
            hashCode = hashCode * -1521134295 + (device == null ? 0 : device.ToString().Length);
            hashCode = hashCode * -1521134295 + player;
            hashCode = hashCode * -1521134295 + input.Length;
            hashCode = hashCode * -1521134295 + mapping.Length;
            return hashCode;
        }
    }
}