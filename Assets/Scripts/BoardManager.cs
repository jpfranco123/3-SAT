using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// This Script (a component of Game Manager) Initializes the Borad (i.e. screen).
public class BoardManager : MonoBehaviour {

	//Resoultion width and Height
	//CAUTION! Modifying this does not modify the Screen resolution. This is related to the unit grid on Unity.
	public static int resolutionWidth = 1024;
	public static int resolutionHeight = 768;

	//Number of Columns and rows of the grid (the possible positions of the Clauses) note: these are default values.
	public static int columns = 4;
	public static int rows = 4;

	//The Clause radius. This is used to avoid superposition of Clauses.
	//public static float KSClauseRadius = 1.5f;

	//Timer width
	//public static float timerWidth =400;

	//A canvas where all the board is going to be placed
	private GameObject canvas;

	//The method to be used to place Clauses randomly on the grid.
	//1. Choose random positions from full grid. It might happen that no placement is found and the trial will be skipped.
	//2. Choose randomly out of 10 positions. A placement is guaranteed

	//Prefab of the Clause interface configuration
	public static GameObject SATClausePrefab;

	//The possible positions of the Clauses;
	private List <Vector3> gridPositions = new List<Vector3> ();

	//Clause and literal vectors for this trial. CURRENTLY SET UP TO ALLOW ONLY INTEGERS.
	//c and ls must be of the same length
	private int[] v;
	private int[] ls;


	//an array where each element takes the value of 0, 1 or 2. an element takes the value of 0 if the literal which has just been added to a clause
	//is on the first button, 1 if the literal is placed on the second button and 2 if the literal is placed on the third button
	private int [] ButtonNumber;

	//an array where each element represents which clause a literal has just been added into. thus, the first 3 elements will be equal to 0, the next 3 elements
	//will be equal to 1, the next 3 elements will be equal to 2 and so on until every literal has been allocated to a clause
	private int [] ClauseNumber;

	//an array where each element represents the state/colour of a corresponding button. the default state takes a value of 0 and corresponds to the colour yellow.
	//if a variable is clicked, all of the corresponding variable/literal pairs will be equal to 1 and the colour will be blue. all of the corresponding variables
	//and opposing literals will be equal to 2 and their colour will be red
	private int [] State;

	//The answer Input by the player
	//0:No / 1:Yes / 2:None
	public static int answer;

	//	private String question;

	//Should the key be working?
	public static bool keysON = false;


	//necessary?
	//If randomization of buttons:
	//1: No/Yes 0: Yes/No
	public static int randomYes;//=Random.Range(0,2);

	public int clickNumber;

	//These variables shouldn't be modified. They just state that the area of the value part of the Clause and the weight part are assumed to be 1.
	//	private static float minAreaBill = 1f;
	//	private static float minAreaWeight = 1f;

	//The total area of all the Clauses. Separated by the value part and the weighy part. A good initialization for this variables is the number of Clauses plus 1.
	//	public static int totalAreaBill = 8;
	//	public static int totalAreaWeight = 8;


	// The list of all the button clicks on Clauses. Each event contains the following information:
	// ItemNumber (a number between 1 and the number of Clauses. It corresponds to the index in the clause's (and literal's) vector.)
	// Clause is being selected In/Out (1/0) 
	// Time of the click with respect to the beginning of the trial 
	//	public List <Vector3> ClauseClicks =  new List<Vector3> ();

	//Structure with the relevant parameters of an Clause.
	//gameClause: is the game object
	//coorValue1: The coordinates of one of the corners of the encompassing rectangle of the Value Part of the Clause. The coordinates are taken relative to the center of the Clause.
	//coorValue2: The coordinates of the diagonally opposite corner of the Value Part of the Clause.
	//coordWeight1 and coordWeight2: Same as before but for the weight part of the Clause.
	//botncitoW: button attached to the weight
	//botncitoV: button attached to the Value (Bill)
	//ItemNumber: a number between 1 and the number of Clauses. It corresponds to the index in the weight's (and value's) vector.
	private struct Clause
	{
		public GameObject gameClause;
		public Vector2 center;
		public int ItemNumber;
	}

	private struct Gumb
	{
		public Button InterfaceGumb;
		public SpriteRenderer ColourGumb;
		public Vector2 gcenter;
		public int gitemnumber;
		public int gstate;
		public int gliteral;
		public int gvariable;
	}

	//The Clauses for the scene are stored here.
	private static Clause[] Clauses;

	private Gumb[] gumbs;

	//This Initializes the GridPositions which are the possible places where the Clauses will be placed.
	void InitialiseList ()
	{
		gridPositions.Clear ();
		//Simple 9 positions grid. 
		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < columns; x++) {	
				float xUnit = (float)(resolutionWidth / 100) / columns;
				float yUnit = (float)(resolutionHeight / 100) / rows;
				//1 x unit = 320x positions in unity, whilst 1 y unit = 336y grid positions in unity
				//gridPositions.Add (new Vector3 ((x-0.8f) * xUnit, (y-0.7619f) * yUnit, 0f)); //- top left value in the right spot, everything else not quite
				gridPositions.Add (new Vector3 ((x) * xUnit, (y+0.4f) * yUnit, 0f));
				Debug.Log ("x" + x + " y" + y);
			}
		}
	}



	//Call only for visualizing grid in the Canvas.
	void seeGrid()
	{
		GameObject hangerpref = (GameObject)Resources.Load ("Hanger");
		for (int ss=0;ss<gridPositions.Count;ss++)
		{
			GameObject hanger = Instantiate (hangerpref, gridPositions[ss], Quaternion.identity) as GameObject;
			canvas=GameObject.Find("Canvas");
			hanger.transform.SetParent (canvas.GetComponent<Transform> (),false);
			hanger.transform.position = gridPositions[ss];
		}
	}



	//Initializes the instance for this trial:
	//1. Sets the question string using the instance (from the .txt files)
	//2. The weight and value vectors are uploaded
	//3. The instance prefab is uploaded
	void setSATInstance()
	{
		int randInstance = GameManager.instanceRandomization[GameManager.TotalTrial-1];

		//		Text Quest = GameObject.Find("Question").GetComponent<Text>();
		//		String question = "Can you obtain at least $" + GameManager.satinstances[randInstance].profit + " with at most " + GameManager.satinstances[randInstance].capacity +"kg?";
		//		Quest.text = question;

		//necessary?
		//question = "Can you pack $" + GameManager.satinstances[randInstance].profit + " if your capacity is " + GameManager.satinstances[randInstance].capacity +"kg?";
		//question = "$" + GameManager.satinstances[randInstance].profit + System.Environment.NewLine + GameManager.satinstances[randInstance].capacity +"kg?";
		//question = " Max: " + System.Environment.NewLine + GameManager.satinstances[randInstance].capacity +"kg ";

		v = GameManager.satinstances [randInstance].variables;
		ls = GameManager.satinstances [randInstance].literals;
		gumbs = new Gumb[v.Length];
		//ButtonNumber = new int[v.Length];
		//ClauseNumber = new int[v.Length];
		//State = new int[v.Length];

		SATClausePrefab = (GameObject)Resources.Load ("SATClause");

	}

	//Shows the question on the screen
	//	public void setQuestion()
	//	{
	//		Text Quest = GameObject.Find("Question").GetComponent<Text>();
	//		Quest.text = question;
	//	}

	/// <summary>
	/// Instantiates an Clause and places it on the position from the input
	/// </summary>
	/// <returns>The Clause structure</returns>
	/// The Clause placing here is temporary; The real placing is done by the placeClause() method.
	Clause generateClause(int ItemNumber ,Vector3 randomPosition)
	{
		//Instantiates the Clause and places it.
		GameObject instance = Instantiate (SATClausePrefab, randomPosition, Quaternion.identity) as GameObject;

		//specifying the literals per clause in case we end up deviating from 3-SAT to k-SAT (where k is not 3)
//		int literalsPerClause = 3;

		canvas=GameObject.Find("Canvas");
		instance.transform.SetParent (canvas.GetComponent<Transform> (),false);

		//Setting the position in a separate line is importatant in order to set it according to global coordinates.
		instance.transform.position = randomPosition;

		//instance.GetComponentInChildren<Text>().text = c[ClauseNumber]+ "Kg \n $" + ls[ClauseNumber];


		//Gets the subcomponents of the Clause, locating each button
		GameObject Button = instance.transform.Find("Button").gameObject;
		GameObject Button1 = instance.transform.Find("Button1").gameObject;
		GameObject Button2 = instance.transform.Find("Button2").gameObject;

		//Button
		//Gumb gumbs[ItemNumber+0] = new Gumb();
		gumbs [ItemNumber].gvariable = v [ItemNumber];
		gumbs [ItemNumber].gliteral = ls[ItemNumber];
		gumbs [ItemNumber].gstate = 0;
		gumbs [ItemNumber].gitemnumber = ItemNumber;
		gumbs [ItemNumber].InterfaceGumb = Button.GetComponent<Button>();
		gumbs [ItemNumber].ColourGumb = Button.GetComponent<SpriteRenderer>();

		//Button1
		//Gumb gumbs[ItemNumber+1] = new Gumb();
		gumbs [ItemNumber+1].gvariable = v [ItemNumber+1];
		gumbs [ItemNumber+1].gliteral = ls[ItemNumber+1];
		gumbs [ItemNumber+1].gstate = 0;
		gumbs [ItemNumber+1].gitemnumber = ItemNumber+1;
		gumbs [ItemNumber+1].InterfaceGumb = Button1.GetComponent<Button>();
		gumbs [ItemNumber+1].ColourGumb = Button1.GetComponent<SpriteRenderer>();

		//Button2
		//Gumb gumbs[ItemNumber+2] = new Gumb();
		gumbs [ItemNumber+2].gvariable = v [ItemNumber+2];
		gumbs [ItemNumber+2].gliteral = ls[ItemNumber+2];
		gumbs [ItemNumber+2].gstate = 0;
		gumbs [ItemNumber+2].gitemnumber = ItemNumber+2;
		gumbs [ItemNumber+2].InterfaceGumb = Button2.GetComponent<Button>();
		gumbs [ItemNumber+2].ColourGumb = Button2.GetComponent<SpriteRenderer>();

//		gumbs[ItemNumber].InterfaceGumb.onClick.AddListener(delegate{ClickDetect(gumbs[ItemNumber]);});
//		gumbs[ItemNumber+1].InterfaceGumb.onClick.AddListener(delegate{ClickDetect(gumbs[ItemNumber+1]);});
//		gumbs[ItemNumber+2].InterfaceGumb.onClick.AddListener(delegate{ClickDetect(gumbs[ItemNumber+2]);});


		string lit;

		//Sets the Text of the first button
		if (ls [ItemNumber] == 0) {
			lit = "-";
		} else {
			lit = "";
		}
		Button.GetComponentInChildren<Text>().text = "" + lit + v[ItemNumber];

		//		//ButtonNumber.length is a multiple of 3. If we divide its length into sets of 3 elements each, this populates the first element of each set with 0
		//		ButtonNumber[ItemNumber] = 0;
		//
		//		//ClauseNumber.length is a multiple of 3. If we divide its length into sets of 3 elements each, this populates the first element of each set
		//		//with the element of the literal divided by the literals per clause. 
		//		ClauseNumber[ItemNumber] = (int)ItemNumber/literalsPerClause;

		//		V[j].Add(new Vector3 (Clauses[i],ButtonNumber[ItemNumber],ls[ItemNumber]));

		//Sets the Text of the second button
		if (ls [ItemNumber+1] == 0) {
			lit = "-";
		} else {
			lit = "";
		}
		Button1.GetComponentInChildren<Text>().text = "" + lit + v[ItemNumber+1];

		//		//This populates the second element of each set in ButtonNumber with 1
		//		ButtonNumber[ItemNumber+1] = 1;
		//
		//		//ensures the second element of each set in ClauseNumber is equal to the first element 
		//		ClauseNumber[ItemNumber+1] = ClauseNumber[ItemNumber];
		//
		////		V.Add(Clauses[i],ButtonNumber[ItemNumber+1],ls[ItemNumber+1]);

		//Sets the Text of the third button
		if (ls [ItemNumber+2] == 0) {
			lit = "-";
		} else {
			lit = "";
		}
		Button2.GetComponentInChildren<Text>().text = "" + lit + v[ItemNumber+2];

		//		//This populates the third element of each set in ButtonNumber with 2
		//		ButtonNumber[ItemNumber+2] = 2;
		//
		//		//ensures the third element of each set in ClauseNumber is equal to the first and second elements 
		//		ClauseNumber[ItemNumber+2] = ClauseNumber[ItemNumber];
		//
		////		V.Add(Clauses[i],ButtonNumber[ItemNumber+2],ls[ItemNumber+2]);


		Clause ClauseInstance = new Clause();
		ClauseInstance.gameClause=instance;
		//		ClauseInstance.coordValue1=new Vector2(-valueW*(1+scale1)/2,0);
		//		ClauseInstance.coordValue2=new Vector2(valueW*(1+scale1)/2,valueH*(1+scale1));
		//		ClauseInstance.coordWeight1=new Vector2(-weightW*(1+scale2)/2,0);
		//		ClauseInstance.coordWeight2=new Vector2(weightW*(1+scale2)/2,-weightH*(1+scale2));


		//Goes from 1 to numberOfClauses
		//note: not sure what this is being used for, so check that's it's ok before using it elsewhere
		ClauseInstance.ItemNumber = ItemNumber+1;

		return(ClauseInstance);

	}


	/// <summary>
	/// Places the Clause on the input position
	/// </summary>
	void placeClause(Clause ClauseToLocate, Vector3 position, int clauseNumber){
		//Setting the position in a separate line is importatant in order to set it according to global coordinates.
		ClauseToLocate.gameClause.transform.position = position;

		gumbs[clauseNumber].InterfaceGumb.onClick.AddListener(delegate{ClickDetect(gumbs[clauseNumber]);});
		gumbs[clauseNumber+1].InterfaceGumb.onClick.AddListener(delegate{ClickDetect(gumbs[clauseNumber+1]);});
		gumbs[clauseNumber+2].InterfaceGumb.onClick.AddListener(delegate{ClickDetect(gumbs[clauseNumber+2]);});

		//ClickDetect (gumbs [clauseNumber]);
	}


	//Returns a random position from the grid and removes the Clause from the list.
	Vector3 RandomPosition()
	{
		int randomIndex=Random.Range(0,gridPositions.Count);
		Vector3 randomPosition = gridPositions[randomIndex];
		gridPositions.RemoveAt(randomIndex);
		return randomPosition;
	}

	//a list of vect
	//	private List<List> V = new List<List> ();
	//
	//	private List <Vector3> CreateVectors()
	//	{
	//	foreach (int j in v) {
	//		V [j] = new List <Vector3> (ClauseNumber, ButtonNumber, ls);
	//	}
	//	}

	// Places all the objects from the instance (v,ls) on the canvas. 
	// Returns TRUE if all Clauses where positioned, FALSE otherwise.
	private bool LayoutObjectAtRandom()
	{
		int objectCount =v.Length;
		//note: not sure what "Clauses" is being used for, so check that's it's ok before using it elsewhere
		Clauses= new Clause[objectCount];
		for(int i=0; i < objectCount;i=i+3)
		{
			Clause ClauseToLocate = generateClause (i, new Vector3 (-1000,-1000,-1000));//66: Change to different Layer?

			Vector3 randomPosition = RandomPosition ();
			placeClause (ClauseToLocate, randomPosition,i);
			ClauseToLocate.center = new Vector2(randomPosition.x,randomPosition.y);
			Clauses [i] = ClauseToLocate;
				
//			while (objectPositioned == 0) 
//			{
//				if (gridPositions.Count > 0) 
//				{
//					Vector3 randomPosition = RandomPosition ();
//					placeClause (ClauseToLocate, randomPosition);
//					ClauseToLocate.center = new Vector2(randomPosition.x,randomPosition.y);
//					Clauses [i] = ClauseToLocate;
//					objectPositioned = 1;
//				}
//				else
//				{
//					//Debug.Log ("Not enough space to place all Clauses");
//					return false;
//				}
//			}

		}

		//call a function which creates a vector for every variable, where that vector contains four pieces of information. it contains an element for every instance of that 
		//variable in the input, and each element has 3 components. 1: which clause it is in, 2: which button it is in, 3: whether the literal is positive or negative
		//CreateVectors();

		return true;
	}

	/// Macro function that initializes the Board
	public void SetupScene(string sceneToSetup)
	{
		if (sceneToSetup == "Trial") 
		{
			itemClicks.Clear ();
			clickNumber = 1;
			//InitialiseList();
			setSATInstance ();
			//If the bool returned by LayoutObjectAtRandom() is false, then retry again:
			//Destroy all Clauses. Initialize list again and try to place them once more.

			//			bool ClausesPlaced = false;
			//				GameObject[] Clauses1 = GameObject.FindGameObjectsWithTag("Clause");
			//				foreach (GameObject Clause in Clauses1)
			//				{
			//					Destroy(Clause);
			//				}

			InitialiseList ();
			//			ClauseClicks.Clear ();
			//seeGrid();
			//		ClausesPlaced = 
			LayoutObjectAtRandom ();
			//		if (ClausesPlaced == false) 
			//		{
			//			GameManager.errorInScene ("Not enough space to place all Clauses");
			//		}
			keysON = true;

		} else if(sceneToSetup == "TrialAnswer")
		{
			answer = 2;
			setSATInstance ();
			//setQuestion ();
			RandomizeButtons ();
			keysON = true;

			//			InitialiseList ();
			//			seeGrid();
		}

	}

	//Updates the timer rectangle size accoriding to the remaining time.
	public void updateTimer()
	{
		// timer = GameObject.Find ("Timer").GetComponent<RectTransform> ();
		// timer.sizeDelta = new Vector2 (timerWidth * (GameManager.tiempo / GameManager.totalTime), timer.rect.height);
		if (GameManager.escena != "SetUp" || GameManager.escena == "InterTrialRest" || GameManager.escena == "End") 
		{
			Image timer = GameObject.Find ("Timer").GetComponent<Image> ();
			timer.fillAmount = GameManager.tiempo / GameManager.totalTime;
		}

	}

	//Sets the triggers for pressing the corresponding keys
	//123: Perhaps a good practice thing to do would be to create a "close scene" function that takes as parameter the answer and closes everything (including keysON=false) and then forwards to 
	//changeToNextScene(answer) on game manager
	//necessary: this was imported from decision version
	private void setKeyInput(){

		if (GameManager.escena == "Trial") {
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				GameManager.saveTimeStamp ("ParticipantSkip");
				GameManager.changeToNextScene (itemClicks,0,0);
			}
		}
		else if (GameManager.escena == "TrialAnswer") 
		{
			//1: No/Yes 0: Yes/No
			if (randomYes == 1) {
				if (Input.GetKeyDown (KeyCode.LeftArrow)) {
					//Left
					//GameManager.changeToNextScene (0, randomYes);
					keysON = false;
					answer=0;
					GameObject boto = GameObject.Find("LEFTbutton") as GameObject;
					highlightButton(boto);
					GameManager.changeToNextScene (itemClicks,0,1);
				} 
				else if (Input.GetKeyDown (KeyCode.RightArrow)) {
					//Right
					//GameManager.changeToNextScene (1, randomYes);
					keysON = false;
					answer=1;
					GameObject boto = GameObject.Find("RIGHTbutton") as GameObject;
					highlightButton(boto);
					GameManager.changeToNextScene (itemClicks,1,1);
				}
			} 
			else if (randomYes == 0) {
				if (Input.GetKeyDown (KeyCode.LeftArrow)) {
					//Left
					//GameManager.changeToNextScene (1, randomYes);
					keysON = false;
					answer=1;
					GameObject boto = GameObject.Find("LEFTbutton") as GameObject;
					highlightButton(boto);
					GameManager.changeToNextScene (itemClicks,1,0);
				} 
				else if (Input.GetKeyDown (KeyCode.RightArrow)) {
					//Right
					//GameManager.changeToNextScene (0, randomYes);
					keysON = false;
					answer = 0;
					GameObject boto = GameObject.Find("RIGHTbutton") as GameObject;
					highlightButton(boto);
					GameManager.changeToNextScene (itemClicks,0,0);
				}
			}
		} 
		else if (GameManager.escena == "SetUp") {
			if (Input.GetKeyDown (KeyCode.Space)) {
				GameManager.setTimeStamp ();
				GameManager.changeToNextScene (itemClicks,0,0);
			}
		}
	}
	private void highlightButton(GameObject butt)
	{
		Text texto = butt.GetComponentInChildren<Text> ();
		texto.color = Color.gray;
	}


	public void setupInitialScreen()
	{
		//Button 
		Debug.Log("Start button");
		GameObject start = GameObject.Find("Start") as GameObject;
		start.SetActive (false);

		//start.btnLeft.GetComponentInChildren<Text>().text = "No";

		InputField pID = GameObject.Find ("ParticipantID").GetComponent<InputField>();

		InputField.SubmitEvent se = new InputField.SubmitEvent();
		//se.AddListener(submitPID(start));
		se.AddListener((value)=>submitPID(value,start));
		pID.onEndEdit = se;


		//pID.onSubmit.AddListener((value) => submitPID(value));

	}

	private void submitPID(string pIDs, GameObject start)
	{
		//Debug.Log (pIDs);

		GameObject pID = GameObject.Find ("ParticipantID");
		GameObject pIDT = GameObject.Find ("Participant ID Text");
		pID.SetActive (false);
		pIDT.SetActive (false);

		//Set Participant ID
		GameManager.participantID=pIDs;

		//Activate Start Button and listener
		//GameObject start = GameObject.Find("Start");
		start.SetActive (true);
		keysON = true;

	}

	public static string getClauseCoordinates()
	{
		string coordinates = "";
		foreach (Clause it in Clauses)
		{
			//Debug.Log ("Clause");
			//Debug.Log (it.center);
			//Debug.Log (it.coordWeight1);
			coordinates = coordinates + "(" + it.center.x + "," + it.center.y + ")";
		}
		return coordinates;
	}

	// Use this for initialization
	void Start () 
	{
//		spriteRenderer = Gumbo.InterfaceGumb.GetComponent<SpriteRenderer>(); // we are accessing the SpriteRenderer that is attached to the Gameobject
//		if (spriteRenderer.sprite == null) // if the sprite on spriteRenderer is null then
//			spriteRenderer.sprite = whitesprite; // set the sprite to sprite1
	}

	// Update is called once per frame
	void Update () 
	{
		if (keysON) 
		{
			setKeyInput ();
		}

	}

	// The list of all the button clicks on items. Each event contains the following information:
	// ItemNumber (a number between 1 and the number of items. It corresponds to the index in the weight's (and value's) vector.)
	// Item is being selected In/Out (1/0) 
	// Time of the click with respect to the beginning of the trial 
	//public static List <Vector4> itemClicks =  new List<Vector4> ();

	public struct itemClick
	{
		public int clickNumber;
		public int gvariable;
		public int gliteral;
		public int state;
		public float time;
	}

	//The Clauses for the scene are stored here.
	public static List <itemClick> itemClicks = new List<itemClick>();


	//public static itemClicks[] =  new List<Vector4> ();

	//Function that takes click as input and stores [c,b]
	private void ClickDetect (Gumb Gumbo)
	{
		if (clickNumber <= GameManager.maxClicks) {

			ChangeState (Gumbo);

			itemClick itemClick1 = new itemClick ();

			Gumb newGumb = gumbs [Gumbo.gitemnumber];
		
			itemClick1.clickNumber = clickNumber;
			itemClick1.gvariable = newGumb.gvariable;
			itemClick1.gliteral = newGumb.gliteral;
			itemClick1.time = GameManager.timeQuestion - GameManager.tiempo;
			itemClick1.state = newGumb.gstate;

			//itemClicks.Add (new Vector4 (clickNumber,Gumbo.gvariable,Gumbo.gliteral,GameManager.timeQuestion - GameManager.tiempo));
			itemClicks.Add (itemClick1);

			clickNumber = clickNumber + 1;
		}
	}

	private void ChangeState(Gumb Gumbo)
	{

		int[] oldstates = new int[gumbs.Length];
		//Populate states with loop
		int[] newstates = new int[gumbs.Length];

		for (int i = 0; i < gumbs.Length; i++) {
			oldstates [i] = gumbs [i].gstate;
		}

		int variableclicked = Gumbo.gvariable;
		int literalclicked = Gumbo.gliteral;

		for (int i = 0; i < v.Length; i++) {
			if (v [i] == variableclicked && ls [i] == literalclicked && oldstates[i] == 0) {
				newstates [i] = 1;
			} else if (v [i] == variableclicked && ls [i] != literalclicked && oldstates[i] == 0) {
				newstates [i] = 2;
			}
			else if (v [i] == variableclicked && oldstates[i] == 1) {
				newstates [i] = 0;
			}
			else if (v [i] == variableclicked && oldstates[i] == 2) {
				newstates [i] = 0;
			}
			else {
				newstates [i] = oldstates [i];
			}
			gumbs [i].gstate = newstates [i];
		}

//		Debug.Log ("State change");
//		Debug.Log (oldstates[Gumbo.gitemnumber]);
//		Debug.Log(newstates[Gumbo.gitemnumber]);
		//66
		ChangeColour (oldstates, newstates);

	}

	//the sprites for each button
	public Sprite whitesprite;
	public Sprite bluesprite;
	public Sprite orangesprite;

//	private SpriteRenderer spriteRenderer; 


	private void ChangeColour (int[] oldstates, int[] newstates)
	{

		for (int i = 0; i < gumbs.Length; i++) {
			if (oldstates [i] != newstates [i]) {
				if (gumbs [i].gstate == 1) { 
					gumbs[i].ColourGumb.sprite = bluesprite;
				} else if (gumbs [i].gstate == 2) {
					gumbs[i].ColourGumb.sprite = orangesprite; 
				} else {
					gumbs[i].ColourGumb.sprite = whitesprite;
				}
			}
		}

	}

//	//the sprites for each button
//	public Image whiteimage;
//	public Image blueimage;
//	public Image orangeimage;
//
//	private SpriteRenderer spriteRenderer; 
//
//
//	private void ChangeColour (Gumb Gumbo, int[] oldstates, int[] newstates)
//	{
//
//		for (int i = 0; i < gumbs.Length; i++) {
//			if (oldstates [i] != newstates [i]) {
//				if (gumbs [i].gstate == 1) { 
//					Gumbo.ColourGumb = blueimage;
//				} else if (gumbs [i].gstate == 2) {
//					Gumbo.ColourGumb = orangeimage; 
//				} else {
//					Gumbo.ColourGumb = whiteimage;
//				}
//			}
//			else {
//			}
//		}
//
//	}

	//necessary?
	//Randomizes YES/NO button positions (left or right) and allocates corresponding script to save the correspondent answer.
	//1: No/Yes 0: Yes/No
	void RandomizeButtons()
	{
		Button btnLeft = GameObject.Find("LEFTbutton").GetComponent<Button>();
		Button btnRight = GameObject.Find("RIGHTbutton").GetComponent<Button>();

		randomYes=GameManager.buttonRandomization[GameManager.trial-1];

		if (randomYes == 1) 
		{
			btnLeft.GetComponentInChildren<Text>().text = "No";
			btnRight.GetComponentInChildren<Text>().text = "Yes";
			//btnLeft.onClick.AddListener(()=>GameManager.changeToNextScene(0));
		} 
		else 
		{
			btnLeft.GetComponentInChildren<Text>().text = "Yes";
			btnRight.GetComponentInChildren<Text>().text = "No";
		}
	}


	//	//Checks if positioning an Clause in the new position generates an overlap. Assuming the new Clause has a radius of KSClauseRadius.
	//	//Returns: TRUE if there is an overlap. FALSE Otherwise.
	//	bool objectOverlapsQ(Vector3 pos)
	//	{
	//		//If physics could be started before update we could use the following easier function:
	//		//bool overlap = Physics2D.IsTouchingLayers(newObject.GetComponent<Collider2D>());
	//
	//		bool overlap = Physics2D.OverlapCircle(pos,KSClauseRadius);
	//		return overlap;
	//
	//	}

	//Checks if positioning an Clause in the new position generates an overlap.
	//Returns: TRUE if there is an overlap. FALSE Otherwise.
	//	bool objectOverlapsQ(Vector3 pos, Clause Clause)
	//	{
	//		Vector2 posxy = new Vector3 (pos.x, pos.y);
	//		bool overlapValue = Physics2D.OverlapArea (Clause.coordValue1+posxy, Clause.coordValue2+posxy);
	//		bool overlapWeight = Physics2D.OverlapArea (Clause.coordWeight1+posxy, Clause.coordWeight2+posxy);

	//Debug.Log ("Clause");
	//Debug.Log(Clause.coordValue1 + posxy);
	//Debug.Log(Clause.coordValue2+posxy);
	//		return overlapValue || overlapWeight;
	//return false;
	//	}


	//necessary: this is what we had from the optimization
	//	private void setKeyInput(){
	//		if (GameManager.escena == "Trial") {
	//			if (Input.GetKeyDown (KeyCode.D)) {
	//				GameManager.changeToNextScene (answer, 1);
	//			} 
	//		} else if (GameManager.escena == "SetUp") {
	//			if (Input.GetKeyDown (KeyCode.D)) {
	//				GameManager.setTimeStamp ();
	//
	//			GameManager.changeToNextScene (0,0);
	//		}
	//	}
	//}
}