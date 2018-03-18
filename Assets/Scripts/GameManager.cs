using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using Random = UnityEngine.Random;
//using System.Diagnostics;

public class GameManager : MonoBehaviour 
{

	//Game Manager: It is a singleton (i.e. it is always one and the same it is nor destroyed nor duplicated)	
	public static GameManager instance=null;

	//The reference to the script managing the board (interface/canvas).
	private BoardManager boardScript;

	//Current Scene
	public static string escena;

	//Time spent so far on this scene
	public static float tiempo;

	//Total time for this scene
	public static float totalTime;

	//Current trial initialization
	public static int trial = 0;

	//The total number of trials across all blocks
	public static int TotalTrial = 0;

	//Current block initialization
	public static int block = 0;

	private static bool showTimer;

	//Intertrial rest time
	public static float timeRest1=5f;

	//The times listed are default settings. They are over-ridden by input files, so you need to change the input files to change times
	//InterBlock rest time
	public static float timeRest2=10;

	//Time given for each trial (The total time the items are shown -With and without the question-)
	public static float timeTrial=10;

	//Time for seeing the SAT question 
	public static float timeQuestion=10;

	//Time given for answering 
	public static float timeAnswer=3;

	//Total number of trials in each block
	private static int numberOfTrials = 30;

	//Total number of blocks
	private static int numberOfBlocks = 3;

	//This is also taken from input files, so in reality 63 instance files are loaded, not 3
	//Number of instance files to be considered. From i1.txt to i_.txt..
	public static int numberOfInstances = 3;

	//The order of the instances to be presented
	public static int[] instanceRandomization;

	//Setting up the variable participantID
	//This is the string that will be used as the file name where the data is stored. DeCurrently the date-time is used.
	public static string participantID = "Empty";

	public static string dateID = @System.DateTime.Now.ToString("dd MMMM, yyyy, HH-mm");

	private static string identifierName;

	//Input and Outout Folders with respect to the Application.dataPath;
	private static string inputFolder = "/DATAinf/Input/";
	private static string inputFolderSATInstances = "/DATAinf/Input/SATInstances/";
	private static string outputFolder = "/DATAinf/Output/";

	//Can copy this code if time stamps are needed (likely) Stopwatch to calculate time of events.
	private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
	// Time at which the stopwatch started. Time of each event is calculated according to this moment.
	private static string initialTimeStamp;

	//The order of the left/right No/Yes randomization
	public static int[] buttonRandomization;

	//we would need a vector of literals, corresponding to 3 literals for each clause in that instance
	//also need a vector of variables for each instance, to see where the literals are being drawn from
	//changes needed further along due to changes in struct
	//A structure that contains the parameters of each instance
	public struct SATInstance
	{
		public int[] variables;
		public int[] literals;

		public int nvariables;
		public int nliterals;

		public string id;
		public string type;

		public int solution;
	}

	//An array of all the instances to be uploaded from .txt files, i.e importing everything using the structure from above 
	public static SATInstance[] satinstances;// = new SATInstance[numberOfInstances];

	// Use this for initialization
	void Awake () 
	{

		//Makes the Game manager a Singleton
		if (instance == null) 
		{
			instance = this;
		}
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);

		//Initializes the game
		boardScript = instance.GetComponent<BoardManager> ();

		InitGame();
		if (escena != "SetUp") 
		{
			saveTimeStamp(escena);
		}

	}

	//Initializes the scene. One scene is setup, other is trial, other is Break....
	void InitGame()
	{
		/*
		Scene Order: escena
		0=setup
		1=trial game
		2=trial game answer
		3= intertrial rest
		4= interblock rest
		5= end
		*/
		//Creates the variable scene? and selects the active scene
		Scene scene = SceneManager.GetActiveScene();
		escena = scene.name;
		//Name the scene "Scene #"
		Debug.Log ("escena" + escena);
		//change numbers to names
		//the loop which runs the game, and drives you from one scene to another
		//If it's the first scene, upload parameters and instances (this happens only once), randomise instances and move incrememntally through >blocks< 1 at a time
		if (escena == "SetUp") 
		{
			block++; 
			loadParameters ();
			loadSATInstance ();

			//RandomizeSATInstances ();
			randomizeButtons ();
			boardScript.setupInitialScreen ();
			//SceneManager.LoadScene (1);

			//If it's the second scene, move incrementally through trials one at a time, set up the question with items only scene from the boardmanager, show the timer and 
			//run it for the time the items should be there by themselves, do not show the question
		} 
		else if (escena == "Trial") 
		{
			trial++;
			TotalTrial++;
			showTimer = true;
			boardScript.SetupScene ("Trial");

			tiempo = timeQuestion;
			totalTime = tiempo;

			//If it's the third scene, set up the question with the 'answer' scene from the boardmanager, show/run the timer for the answer
		} 
		else if (escena == "TrialAnswer") 
		{
			showTimer = true;
			boardScript.SetupScene ("TrialAnswer");
			tiempo = timeAnswer;
			totalTime = tiempo;

			//If it's the fourth scene, don't show the timer and run it for the time between trials
		} 
		else if (escena == "InterTrialRest") 
		{
			showTimer = false;
			tiempo = timeRest1;
			totalTime = tiempo;

			//If it's the fifth scene, show the timer and run it for the time between blocks, then proceed to the next block
		} 
		else if (escena == "InterBlockRest") 
		{
			trial = 0;
			block++;
			showTimer = true;
			tiempo = timeRest2;
			totalTime = tiempo;
			//Debug.Log ("TiempoRest=" + tiempo);

			randomizeButtons ();
			//SceneManager.LoadScene (1);
		}

	}


	// Update is called once per frame
	void Update () 
	{

		if (escena != "SetUp") 
		{
			startTimer ();
			//			pauseManager ();
		}
	}





	//instances info .txt files seem to be working perfectly, trial info mistakenly labels each instance as the on after it, timestamps don't work at all



	/// <summary>
	/// Saves the headers for both files (Trial Info and Time Stamps)
	/// In the trial file it saves:  1. The participant ID. 2. Instance details.
	/// In the TimeStamp file it saves: 1. The participant ID. 2.The time onset of the stopwatch from which the time stamps are measured. 3. the event types description.
	/// </summary>
	private static void saveHeaders()
	{
		//name of the file and where to save it
		identifierName = participantID + "_" + dateID + "_" + "SAT" + "_";
		string folderPathSave = Application.dataPath + outputFolder;

		//Saves instance info
		//an array of string, a new variable called lines3
		string[] lines3 = new string[numberOfInstances+2];
		//the first two lines will show the following - "string": "parameter/input"
		lines3[0]="PartcipantID:" + participantID;
		lines3 [1] = "instanceNumber" + ";v" + ";l" + ";id" + ";type" + ";sol";

		int l = 2;
		int satn = 1;
		foreach (SATInstance sat in satinstances) 
		{
			//Without instance type and problem ID:
			//lines [l] = "Instance:" + satn + ";c=" + sat.capacity + ";p=" + sat.profit + ";w=" + string.Join (",", sat.variables.Select (p => p.ToString ()).ToArray ()) + ";v=" + string.Join (",", sat.literals.Select (p => p.ToString ()).ToArray ());
			//With instance type and problem ID
			lines3 [l] = satn + ";" + string.Join (",", sat.variables.Select (p => p.ToString ()).ToArray ()) + ";" + string.Join (",", sat.literals.Select (p => p.ToString ()).ToArray ())
				+ ";" + sat.id + ";" + sat.type + ";" + sat.solution;

			l++;
			satn++;
		}
		//using StreamWriter to write the above into an output file
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "InstancesInfo.txt",true)) 
		{
			foreach (string line in lines3)
				outputFile.WriteLine(line);
		}


		// Trial Info file headers
		string[] lines = new string[2];
		lines[0]="PartcipantID:" + participantID;
		lines [1] = "block;trial;answer;correct;timeSpent;randomYes(1=Left:No/Right:Yes);instanceNumber;error";
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TrialInfo.txt",true)) 
		{
			foreach (string line in lines)
				outputFile.WriteLine(line);
		}


		// Time Stamps file headers
		string[] lines1 = new string[3];
		lines1[0]="PartcipantID:" + participantID;
		lines1[1] = "InitialTimeStamp:" + initialTimeStamp;
		lines1[2]="block;trial;instanceNumber;eventType;elapsedTime";
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TimeStamps.txt",true)) 
		{
			foreach (string line in lines1)
				outputFile.WriteLine(line);
		}

		//Headerds for Clicks file
		string[] lines2 = new string[3];
		lines2[0]="PartcipantID:" + participantID;
		lines2[1] = "InitialTimeStamp:" + initialTimeStamp;
		lines2[2]="block;trial;clicknumber;Variable;Literal;time"; 
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "Clicks.txt",true)) {
			foreach (string line in lines2)
				outputFile.WriteLine(line);
		}

	}


	//Saves the data of a trial to a .txt file with the participants ID as filename using StreamWriter.
	//If the file doesn't exist it creates it. Otherwise it adds on lines to the existing file.
	//Each line in the File has the following structure: "trial;answer;timeSpent".
	public static void save(int answer, float timeSpent, int randomYes, string error) 
	{
		//disregard this...string xyCoordinates = instance.boardScript.getItemCoordinates ();//BoardManager.getItemCoordinates ();

		//Get the instance number for this trial (take the block number, subtract 1 because indexing starts at 0. Then multiply it
		//by numberOfTrials (i.e. 10, 10 per block) and add the trial number of this block. Thus, the 2nd trial of block 2 will be
		//instance number 12 overall) and add 1 because the instanceRandomization is linked to array numbering in C#, which starts at 0;
		int instanceNum = instanceRandomization [TotalTrial - 1] + 1;

		//looks at the solution, it is either correct or incorrect
		int solutionQ = satinstances [instanceNum - 1].solution;
		int correct = (solutionQ == answer) ? 1 : 0;

		//what to save and the order in which to do so
		//		string dataTrialText = /* block + ";" + */trial + ";" + answer + ";" + correct + ";" + timeSpent + ";" + randomYes +";" + instanceNum + ";" + error;
		string dataTrialText = block + ";" + trial + ";" + answer + ";" + correct + ";" + timeSpent + ";" + randomYes +";" + instanceNum + ";" + error;

		//where to save
		string[] lines = {dataTrialText};
		string folderPathSave = Application.dataPath + outputFolder;

		//This location can be used by unity to save a file if u open the game in any platform/computer:      Application.persistentDataPath;

		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName +"TrialInfo.txt",true)) 
		{
			foreach (string line in lines)
				outputFile.WriteLine(line);
		}

		//Options of streamwriter include: Write, WriteLine, WriteAsync, WriteLineAsync
	}



	/// <summary>
	/// Saves the time stamp for a particular event type to the "TimeStamps" File
	/// All these saves take place in the Data folder, where you can create an output folder
	/// </summary>
	/// Event type: 1=ItemsNoQuestion;11=ItemsWithQuestion;2=AnswerScreen;21=ParticipantsAnswer;3=InterTrialScreen;4=InterBlockScreen;5=EndScreen
	public static void saveTimeStamp(string eventType) 
	{
		//				string dataTrialText = /* block + ";" + */ trial + ";" + eventType + ";" + timeStamp();
		string dataTrialText = block + ";" + trial + ";" + eventType + ";" + timeStamp();

		string[] lines = {dataTrialText};
		string folderPathSave = Application.dataPath + outputFolder;

		//This location can be used by unity to save a file if u open the game in any platform/computer:      Application.persistentDataPath;
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TimeStamps.txt",true)) 
		{
			foreach (string line in lines)
				outputFile.WriteLine(line);
		}

	}





	/// <summary>
	/// Saves the time stamp of every click made on the items 
	/// </summary>
	/// block ; trial ; clicklist (i.e. item number ; itemIn? (1: selcting; 0:deselecting) ; time of the click with respect to the begining of the trial)
	public static void saveClicks(List<Vector4> clicksList) {

		string folderPathSave = Application.dataPath + outputFolder;


		string[] lines = new string[clicksList.Count];
		int i = 0;
		foreach (Vector4 clickito in clicksList) {
			lines[i]= block + ";" + trial + ";" + clickito.x + ";" + clickito.y + ";" + clickito.z + ";" + clickito.w ;
			i++;
		}
		//This location can be used by unity to save a file if u open the game in any platform/computer:      Application.persistentDataPath;
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "Clicks.txt",true)) {
			foreach (string line in lines)
				outputFile.WriteLine(line);
		} 

	}









	/*
	 * Loads all of the instances to be uploaded form .txt files. Example of input file:
	 * Name of the file: i3.txt
	 * Structure of each file is the following:
	 * variables:[1,2,3,4,5]
	 * literals:[X,Y,D,G,W]
	 *
	 * The instances are stored as satinstances structures in the array of structures: satinstances
	 */
	public static void loadSATInstance()
	{
		//string folderPathLoad = Application.dataPath.Replace("Assets","") + "DATA/Input/KPInstances/";
		string folderPathLoad = Application.dataPath + inputFolderSATInstances;
		//		int linesInEachKPInstance = 4;
		satinstances = new SATInstance[numberOfInstances];

		for (int k = 1; k <= numberOfInstances; k++) {
			//create a dictionary where all the variables and definitions are strings
			var dict = new Dictionary<string, string> ();
			//			//Use streamreader to read the input files if there are lines to read
			//string[] KPInstanceText = new string[linesInEachKPInstance];
			try {   // Open the text file using a stream reader.
				using (StreamReader sr = new StreamReader (folderPathLoad + "i"+ k + ".txt")) {
					//					for (int i = 0; i < linesInEachKPInstance; i++) {
					//						string line = sr.ReadLine ();
					//						string[] dataInLine = line.Split (':');
					//						SATInstanceText [i] = dataInLine [1];
					//					}
					string line;
					while (!string.IsNullOrEmpty ((line = sr.ReadLine ()))) {
						string[] tmp = line.Split (new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
						// Add the key-value pair to the dictionary:
						dict.Add (tmp [0], tmp [1]);//int.Parse(dict[tmp[1]]);
					}
					// Read the stream to a string, and write the string to the console.
					//String line = sr.ReadToEnd();
				}
				//if there is a problem, then report the following error message
			} catch (Exception e) {
				Debug.Log ("The file could not be read:");
				Debug.Log (e.Message);
			}
			//the following are all recorded as string (hence the S at the end) 
			string variablesS;
			string literalsS;
			string nvariablesS;
			string nliteralsS;
			string solutionS;

			//grab all of those parameters as strings
			dict.TryGetValue ("variables", out variablesS);
			dict.TryGetValue ("literals", out literalsS);
			dict.TryGetValue ("nvariables", out nvariablesS);
			dict.TryGetValue ("nliterals", out nliteralsS);
			dict.TryGetValue ("solution", out solutionS);


			//convert (most of them) to integers, with variables and literals being arrays and the others single literals
			satinstances [k-1].variables = Array.ConvertAll (variablesS.Substring (1, variablesS.Length - 2).Split (','), int.Parse);
			satinstances [k-1].literals = Array.ConvertAll (literalsS.Substring (1, literalsS.Length - 2).Split (','), int.Parse);
			satinstances [k-1].nvariables = int.Parse (nvariablesS);
			satinstances [k-1].nliterals = int.Parse (nliteralsS);
			satinstances [k-1].solution = int.Parse (solutionS);

			dict.TryGetValue ("problemID", out satinstances [k - 1].id);
			dict.TryGetValue ("ratio", out satinstances [k - 1].type);
		}
	}

	//Loads the parameters from the text files: param.txt and layoutParam.txt
	void loadParameters()
	{
		//string folderPathLoad = Application.dataPath.Replace("Assets","") + "DATA/Input/";
		string folderPathLoad = Application.dataPath + inputFolder;
		var dict = new Dictionary<string, string>();

		try 
		{   // Open the text file using a stream reader.
			using (StreamReader sr = new StreamReader (folderPathLoad + "layoutParam.txt")) 
			{

				// (This loop reads every line until EOF or the first blank line.)
				string line;
				while (!string.IsNullOrEmpty((line = sr.ReadLine())))
				{
					// Split each line around ':'
					string[] tmp = line.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
					// Add the key-value pair to the dictionary:
					dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
				}
			}


			using (StreamReader sr1 = new StreamReader (folderPathLoad + "param.txt")) 
			{

				// (This loop reads every line until EOF or the first blank line.)
				string line1;
				while (!string.IsNullOrEmpty((line1 = sr1.ReadLine())))
				{
					//Debug.Log (1);
					// Split each line around ':'
					string[] tmp = line1.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
					// Add the key-value pair to the dictionary:
					dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
				}
			}

			using (StreamReader sr2 = new StreamReader (folderPathLoad + "param2.txt"))
			{

				// (This loop reads every line until EOF or the first blank line.)
				string line2;
				while (!string.IsNullOrEmpty((line2 = sr2.ReadLine())))
				{
					//Debug.Log (1);
					// Split each line around ':'
					string[] tmp = line2.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
					// Add the key-value pair to the dictionary:
					dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
				}
			}

		} 
		catch (Exception e) 
		{
			Debug.Log ("The file could not be read:");
			Debug.Log (e.Message);
		}

		assignVariables(dict);

	}

	//Assigns the parameters in the dictionary to variables
	void assignVariables(Dictionary<string,string> dictionary)
	{
		//Assigns Parameters - these are all going to be imported from input files
		string timeRest1S;
		string timeRest2S;
		string timeQuestionS;
		string timeAnswerS;
		string numberOfTrialsS;
		string numberOfBlocksS;
		string numberOfInstancesS;
		string instanceRandomizationS;

		dictionary.TryGetValue ("timeRest1", out timeRest1S);
		dictionary.TryGetValue ("timeRest2", out timeRest2S);

		dictionary.TryGetValue ("timeQuestion", out timeQuestionS);

		dictionary.TryGetValue ("timeAnswer", out timeAnswerS);

		dictionary.TryGetValue ("numberOfTrials", out numberOfTrialsS);

		dictionary.TryGetValue ("numberOfBlocks", out numberOfBlocksS);

		dictionary.TryGetValue ("numberOfInstances", out numberOfInstancesS);

		timeRest1=Convert.ToSingle (timeRest1S);
		timeRest2=Convert.ToSingle (timeRest2S);
		timeQuestion=Int32.Parse(timeQuestionS);
		timeAnswer=Int32.Parse(timeAnswerS);
		numberOfTrials=Int32.Parse(numberOfTrialsS);
		numberOfBlocks=Int32.Parse(numberOfBlocksS);
		numberOfInstances=Int32.Parse(numberOfInstancesS);

		dictionary.TryGetValue ("instanceRandomization", out instanceRandomizationS);
		//If instanceRandomization is not included in the parameters file. It generates a randomization.
		//		if (!dictionary.ContainsKey("instanceRandomization")){
		//			RandomizeKSInstances();
		//		} else{
		int[] instanceRandomizationNo0 = Array.ConvertAll(instanceRandomizationS.Substring (1, instanceRandomizationS.Length - 2).Split (','), int.Parse);
		instanceRandomization = new int[instanceRandomizationNo0.Length];
		//foreach (int i in instanceRandomizationNo0)
		for (int i = 0; i < instanceRandomizationNo0.Length; i++)
		{
			instanceRandomization[i] = instanceRandomizationNo0 [i] - 1;
		}
		//		}


		//Necessary?
		////Assigns LayoutParameters
		//string resolutionWidthS;
		//string resolutionHeightS;
		string columnsS;
		string rowsS;
		//string KSItemRadiusS;
		//string totalAreaBillS;
		//string totalAreaWeightS;

		//dictionary.TryGetValue ("resolutionWidth", out resolutionWidthS);
		//dictionary.TryGetValue ("resolutionHeight", out resolutionHeightS);
		dictionary.TryGetValue ("columns", out columnsS);
		dictionary.TryGetValue ("rows", out rowsS);
		//	dictionary.TryGetValue ("totalAreaBill", out totalAreaBillS);
		//	dictionary.TryGetValue ("totalAreaWeight", out totalAreaWeightS);

		//dictionary.TryGetValue ("KSItemRadius", out KSItemRadiusS);


		//BoardManager.resolutionWidth=Int32.Parse(resolutionWidthS);
		//BoardManager.resolutionHeight=Int32.Parse(resolutionHeightS);
		BoardManager.columns=Int32.Parse(columnsS);
		BoardManager.rows=Int32.Parse(rowsS);
		//	BoardManager.totalAreaBill=Int32.Parse(totalAreaBillS);
		//	BoardManager.totalAreaWeight=Int32.Parse(totalAreaWeightS);
		//BoardManager.KSItemRadius=Convert.ToSingle(KSItemRadiusS);//Int32.Parse(KSItemRadiusS);
	}





	//66: Wrong function: items are repeated.
	//Randomizes the sequence of Instances to be shown to the participant adn stores it in: instanceRandomization
	void RandomizeSATInstances()
	{
		//		instanceRandomization = new int[numberOfTrials/**numberOfBlocks*/];
		//		for (int i = 0; i < numberOfTrials/**numberOfBlocks*/; i++) 
		instanceRandomization = new int[numberOfTrials*numberOfBlocks];
		for (int i = 0; i < numberOfTrials*numberOfBlocks; i++) 
		{
			instanceRandomization[i] = Random.Range(0,numberOfInstances);
		}
	}


	//Takes care of changing the Scene to the next one (Except for when in the setup scene)
	public static void changeToNextScene(List <Vector4> itemClicks, int answer, int randomYes)
	{
		BoardManager.keysON = false;
		if (escena == "SetUp") {
			Debug.Log ("SetUp");
			saveHeaders ();
			SceneManager.LoadScene ("Trial");
		}
		else if (escena == "Trial") {
			SceneManager.LoadScene ("TrialAnswer");
		} 
		else if (escena == "TrialAnswer") {
			if (answer == 2) {
				save (answer, timeQuestion, randomYes, "");
			} else {
				save (answer, timeAnswer - tiempo, randomYes, "");
				saveTimeStamp ("ParticipantAnswer");
			}
			saveClicks (itemClicks);
			SceneManager.LoadScene ("InterTrialRest");
		} else if (escena == "InterTrialRest") {
			changeToNextTrial ();
		} else if (escena == "InterBlockRest") {
			Debug.Log ("Hi");

			SceneManager.LoadScene ("Trial");
		}
	}
	//Redirects to the next scene depending if the trials or blocks are over.
	private static void changeToNextTrial()
	{
		//Checks if trials are over
		if (trial < numberOfTrials) {
			SceneManager.LoadScene ("Trial");
		} else if (block < numberOfBlocks) {
			SceneManager.LoadScene ("InterBlockRest");
		}else {
			SceneManager.LoadScene ("End");
		}
	}

	//	/// <summary>
	//	/// Extracts the items that were finally selected based on the sequence of clicks.
	//	/// </summary>
	//	/// <returns>The items selected.</returns>
	//	/// <param name="itemClicks"> Sequence of clicks on the items.</param>
	//	private static string extractItemsSelected (List <Vector4> itemClicks){
	//		List<int> itemsIn = new List<int>();
	//		foreach(Vector4 clickito in itemClicks){
	//			if (clickito.z == 1) {
	//				itemsIn.Add (Convert.ToInt32 (clickito.x));
	//			} else if (clickito.z == 0) {
	//				itemsIn.Remove (Convert.ToInt32 (clickito.x));
	//			} else if (clickito.z == 3) {
	//				itemsIn.Clear ();
	//			}
	//		}
	//
	//		string itemsInS = "";
	//		foreach (int i in itemsIn)
	//		{
	//			itemsInS = itemsInS + i + ",";
	//		}
	//		if(itemsInS.Length>0)
	//			itemsInS = itemsInS.Remove (itemsInS.Length - 1);
	//
	//		return itemsInS;
	//	}
	//
	//

	//Randomizes The Location of the Yes/No button for a whole block.
	void randomizeButtons()
	{
		buttonRandomization = new int[numberOfTrials];
		List<int> buttonRandomizationTemp = new List<int> ();
		for (int i = 0; i < numberOfTrials; i++){
			if (i % 2 == 0) {
				buttonRandomizationTemp.Add(0);
			} else {
				buttonRandomizationTemp.Add(1);
			}
		}
		for (int i = 0; i < numberOfTrials; i++) {
			int randomIndex = Random.Range (0, buttonRandomizationTemp.Count);
			buttonRandomization [i] = buttonRandomizationTemp [randomIndex];
			buttonRandomizationTemp.RemoveAt (randomIndex);
		}
	}



	/// <summary>
	/// Starts the stopwatch. Time of each event is calculated according to this moment.
	/// Sets "initialTimeStamp" to the time at which the stopwatch started.
	/// </summary>
	public static void setTimeStamp()
	{
		initialTimeStamp=@System.DateTime.Now.ToString("HH-mm-ss-fff");
		stopWatch.Start ();
		Debug.Log (initialTimeStamp);
	}

	/// <summary>
	/// Calculates time elapsed
	/// </summary>
	/// <returns>The time elapsed in milliseconds since the "setTimeStamp()".</returns>
	private static string timeStamp()
	{
		//		TimeSpan ts = stopWatch.Elapsed;
		//		string stamp = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
		//			ts.Hours, ts.Minutes, ts.Seconds,
		//			ts.Milliseconds / 10);
		long milliSec = stopWatch.ElapsedMilliseconds;
		string stamp = milliSec.ToString();
		return stamp;
	}


	//Updates the timer (including the graphical representation)
	//If time runs out in the trial or the break scene. It switches to the next scene.
	void startTimer()
	{
		tiempo -= Time.deltaTime;
		//Debug.Log (tiempo);
		if (showTimer) 
		{
			boardScript.updateTimer();
			//	RectTransform timer = GameObject.Find ("Timer").GetComponent<RectTransform> ();
			//	timer.sizeDelta = new Vector2 (timerWidth * (tiempo / timeTrial), timer.rect.height);
		}

		//When the time runs out:
		if(tiempo < 0)
		{
			//changeToNextScene(2,BoardManager.randomYes);
			changeToNextScene(BoardManager.itemClicks,BoardManager.answer,BoardManager.randomYes);
		}
	}

	/// <summary>
	/// In case of an error: Skip trial and go to next one.
	/// Example of error: Not enough space to place all items
	/// </summary>
	/// Receives as input a string with the errorDetails which is saved in the output file.
	public static void errorInScene(string errorDetails){
		Debug.Log (errorDetails);

		BoardManager.keysON = false;
		int answer = 3;
		int randomYes = -1;
		save (answer, timeTrial, randomYes, errorDetails);
		changeToNextTrial ();
	}


}

/* 
 * using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour 
{
	public Image Bar;
	public float max_time = 10f;
	public float cur_time = 0f;

	// Use this for initialization
	void Start () 
	{
		cur_time = max_time;
		InvokeRepeating ("decreaseTime", 0f, 1f);
	}

	void decreaseTime()
	{
		cur_time -= 1f;
		float calc_time = cur_time / max_time;
		SetTime (calc_time);
	}

	void SetTime(float mytime)
	{
		Bar.fillAmount = mytime;
	}

} 
*/
