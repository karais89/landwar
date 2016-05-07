using UnityEngine;
using System.Collections;

public class CMainTitle : MonoBehaviour {
	
	Texture bg;
	GameObject battleroom;

	// Use this for initialization
	void Start () {
		// Resources.Load 함수를 쓸 때 리소스파일은 반드시 "Assets/Resources" 폴더 하위에 존재해야 합니다.
		this.bg = Resources.Load("images/title_blue") as Texture;
		
		this.battleroom = GameObject.Find("BattleRoom");
		this.battleroom.SetActive(false);		 
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0))
		{
			// 마우스 버튼이 눌리면 battleroom 오브젝트를 활성화 시키고,
			// 현재 오브젝트를 비활성화 시켜서 room 화면으로 전환시켜 줍니다.
			
			// 게임오브젝트가 활성화 되면 거기에 붙은 스크립트 Start() 함수가 호출되면서 작동이 시작되고,
			// 비활성화 되면 스크립트 역시 작동을 멈춥니다.
			
			// 따라서 각각의 화면들을 오브젝트로 만들고 그에따른 스크립트를 붙여주면
			// 각 화면에 따른 코드들을 여러 파일에 분리하여 관리할 수 있게 됩니다.
			this.battleroom.SetActive(true);
			this.gameObject.SetActive(false);
		}
	}
	
	// 유니티 4.6이상 버전 부터는 레거시 되었지만 일단 강좌에서 이걸 사용하기 때문에 사용.
	void OnGUI()
	{
		// 그냥 그립니다 화면 크기대로.
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), this.bg);
	}
}
