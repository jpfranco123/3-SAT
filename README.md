# 3-SAT (Boolean satisfiability) behavioural task on unity(C#).

This task is based on the 3-satisfiability problem (3SAT). In this problem, the aim is to determine whether a boolean formula is satisfiable. In other words, given a propositional formula, the aim is to determine whether there exists at least one configuration of the variables (which can take values TRUE or FALSE) such that the formula evaluates to TRUE. The propositional formula in 3SAT has a specific structure. Specifically, the formula is composed of a conjunction of clauses that must all evaluate TRUE for the whole formula to evaluate TRUE. Each of these clauses, takes the form of an OR logical operator of three literals (variables and their negations).

In order to represent this in an accessible way to participants we developed a task composed of switches and light bulbs. Participants were presented with a set of light bulbs (clauses), each of which had three switches underneath (literals) that were represented by a positive or negative number. The number on each switch represented the variable number, which could be turned on or off (TRUE or FALSE). The aim of the task is to determine whether there exists a way of turning on and off variables such that all the light bulbs are turned on (that is, the formula evaluates TRUE).

Each trial presents a different instance of SAT. At the beginning of each trial, participants are presented with a different instance of the 3SAT problem. A bar in the top-right corner of the screen indicated the time remaining in the trial. Trials are self-paced with a time limit. Participants can use the mouse to click on any of the variables to select their value (blue=TRUE,orange=FALSE). A light bulb above each clause indicates whether a clause evaluates to TRUE (light on) given the selected values of the variables underneath it. The number of clicks in each trial has a limit chosen by the experimenter. When participants are ready to submit their solution, they press a button to advance from the screen displaying the instance to the response screen where they responded YES or NO. There is a time limit to respond.

## SETUP

Input/Output data folders are located in Assets/DATAinf. This folder has to be added manually to the game after building.

This is the structure of the folder:
- DataInf
	- Output
	- Input 
		- layoutParam.txt
		- param.txt
		- SATInstances
			- i1.txt
			- i2.txt 
				…
			- 1_param2.txt
			- 2_param2.txt
				…

### Description of INPUT files:

Input Files: param.txt, n_param2.txt(n=1,2,…), layoutParam.txt, Instances/i1.txt…

The main structure of these files is: 
NameOfTheVariable1:Value1
NameOfTheVariable2:Value2
…

**layoutParam.txt**

Relevant Parameters for the layout of the screen. All Variables must be INTEGERS.

**param.txt**

Relevant Parameters of the task. All Variables must be INTEGERS or vectors of INTEGERS.
timeRest1:=Time for the inter-trial Break.
timeRest2:=Time for the inter-blocks Break.
timeQuestion:=Time given for each trial (The total time the items are shown.
timeAnswer:=Time given to answer.
maxClicks:=Maximum number of clicks allowed.

**1_param2.txt,2_param2.txt...**

Variables can be allocated between param.txt and param2.txt with no effect on the game; however there must not be repeated definitions of variables. The distinction is done because param2.txt is an output from the instance selection program (e.g python).
numberOfInstances:=Number of instances to be imported. The files uploaded are 			automatically i1-i”numberOfInstances”
numberOfBlocks:=Number of blocks.
numberOfTrials:=Number of trials in each block.
instanceRandomization:=Sequence of instances to be randomised. The vector must have length: 	trials*blocks. E.g. [1,3,2,3,1,3,1,2,3,1] for 2 blocks of 5 trials.


**i1.txt,i2.txt,…**

Instance information. Each file is a different instance of the problem. 
Files must be added sequentially (i.e. 1,2,3,…). Except for “param” all Variables must not be floats (i.e. integers, strings…)

Example:
variables:[1, 2, 4, 2, 3, 4, 1, 2, 4, 2, 3, 4, 2, 3, 5, 1, 2, 3, 1, 4, 5, 3, 4, 5, 1, 3, 5, 3, 4, 5, 1, 2, 5, 1, 2, 5, 1, 3, 4, 2, 3, 5, 2, 3, 4, 1, 2, 5, 1, 2, 4, 1, 4, 5]
literals:[1, 1, 1, 1, 1, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 1, 0, 0, 0, 1, 1, 0, 0]
nvariables:5
nliterals:54
nclauses:18
ratio:3.6
problemId:3.607-279
solution:1
type:6


## Game controls:

- Type your ID and click enter.
- Type a randomisation number from 1 to 50 (this depends in the number of i_param2.txt files you have.) and click enter.
- Click space bar in order to start.
- Click on literals to select possible solutions to the 3-SAT.
- Press UP arrow to go to answer submission screen.
- Press LEFT / RIGHT Arrow to choose an answer.


