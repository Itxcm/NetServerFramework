# NetServerFramework

I want to build a framework where a client only needs to send messages and receive responses from the server.

## Configuration

### Client

- I will write it once there is more information available.

### Server

- I will write it once there is more information available.

## Example

- The client can send data to the server by calling the Service layer and receive data through callbacks.

  ```c#
  // Register the callback for the server.
  private void Awake()
  {
      UserService.Instance.LoginCall = LoginCall;
      UserService.Instance.RegisterCall = RegisterCall;
  }
  // Simply call the API to send a request to the server.
  private void BtnListen()
  {
      reg.GetComponent<Button>().onClick.AddListener(() => UserService.Instance.Register(userName.text, password.text));
      log.GetComponent<Button>().onClick.AddListener(() => UserService.Instance.Login(userName.text, password.text));
  }
  // Login server response.
  private void LoginCall(Result res, string msg)
  {
      if (res == Result.Success)
      {
          gameObject.SetActive(false);
          LoginView.Instance.SetNameActive(true, "Default_login");
      }
      TipsConfig.Instance.ShowSystemTips(msg);
  }
  // Registration server response.
  private void RegisterCall(Result res, string msg)
  {
      if (res == Result.Success) SetInputActive(true);
      TipsConfig.Instance.ShowSystemTips(msg);
  }
  ```

- We retrieve data stored in the database or cache by accessing the User data layer provided by the server.

  ```c#
  // Get user data.
  private void GetUserData()
  {
      // Data will only be available once you have successfully logged in, otherwise it will remain null.
      NUserInfo userInfo = User.Instance.UserInfo;
  }
  ```

  

## TODO

- I will write it once there is more information available.

