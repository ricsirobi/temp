public interface InputProcessor
{
	bool Update(GameInput inInput);

	void LateUpdate(GameInput inInput);

	bool IsPressed();

	bool IsUp();

	bool IsDown();

	void Calibrate();

	void SetCalibration(float x, float y, float z);
}
