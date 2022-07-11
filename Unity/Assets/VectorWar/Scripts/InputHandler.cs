using UnityEngine;
using IR = InputRegistry;

public class InputHandler : MonoBehaviour {
    public static InputHandler ih;
    public long lastInputs = 0;

    void Awake() {
        EnforceSingleton();

        // Subscribe();
    }
    void OnDestroy() {
        // Unsubscribe();
    }

    private void EnforceSingleton() {
        //Singleton enforcement
        if (!(ih is null))
            Destroy(this);
        ih = this;
    }

    /// <summary>
    /// Reads from InputRegistry's default PlayerControls and assigns them to it
    /// </summary>
    public long ReadInputs(long lastInputs) {
        long input = 0;

        // Check up input
        if (IR.CheckPlayerInput(IR.INPUT_UP_NM, 1)) {
            // We're pressing this frame
            input |= IR.INPUT_UP;
            // Was it not pressed last frame?
            if ((lastInputs & IR.INPUT_UP) == 0 && (lastInputs & IR.INPUT_UP_THIS_FRAME) == 0) {
                // Then it was pressed down this frame
                input |= IR.INPUT_UP_THIS_FRAME;
            }
        }

        // Check left input
        if (IR.CheckPlayerInput(IR.INPUT_LEFT_NM, 1)) {
            input |= IR.INPUT_LEFT;
            if ((lastInputs & IR.INPUT_LEFT) == 0 && (lastInputs & IR.INPUT_LEFT_THIS_FRAME) == 0) {
                input |= IR.INPUT_LEFT_THIS_FRAME;
            }
        }

        // Check right input
        if (IR.CheckPlayerInput(IR.INPUT_RIGHT_NM, 1)) {
            input |= IR.INPUT_RIGHT;
            if ((lastInputs & IR.INPUT_RIGHT) == 0 && (lastInputs & IR.INPUT_RIGHT_THIS_FRAME) == 0) {
                input |= IR.INPUT_RIGHT_THIS_FRAME;
            }
        }

        // Check down input
        if (IR.CheckPlayerInput(IR.INPUT_DOWN_NM, 1)) {
            input |= IR.INPUT_DOWN;
            if ((lastInputs & IR.INPUT_DOWN) == 0 && (lastInputs & IR.INPUT_DOWN_THIS_FRAME) == 0) {
                input |= IR.INPUT_DOWN_THIS_FRAME;
            }
        }

        return input;
    }
}
