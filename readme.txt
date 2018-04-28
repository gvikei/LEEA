-------------
1. LICENSE
-------------

This program is free software; you can redistribute it and/or modify it
under the terms of the GNU General Public License version 2 as published
by the Free Software Foundation (LGPL may be granted upon request). This program is
distributed in the hope that it will be useful, but without any warranty; without even
the implied warranty of merchantability or fitness for a particular purpose. See the
GNU General Public License for more details.

---------------------
2. USAGE
---------------------

INTRODUCTION
---------------------
This package implements the LEEA algorithm as described in the GECCO 2016 LEEA paper
(http://eplex.cs.ucf.edu/papers/morse_gecco16.pdf) and is intended to provide a
useful starting point for those interested in running experiments with LEEA, 
modifying the LEEA algorithm, or applying LEEA to new domains.

SOFTWARE INFO
---------------------
The software package is written in C# and is packaged as a Visual Studio 2010 solution.  
The package uses some classes from SharpNeat v2, though the evolution algorithm has been rewritten.

RUNNING LEEA
---------------------
To run the program without any modifications, browse to LEEA/LEEA/bin/x64/Debug.
The executable is named LEEA.exe and you can override the default parameters 
by creating a file named params.txt.
There are four sample params text files that may be renamed in order to run each of the four
available algorithms on the time series equation described in the GECCO 2016 LEEA paper.


PARAMETER MODIFICATION
---------------------
Many parameters may be modified by only changing the params.txt file.  This allows one to
perform experiments without the need to modify code.
The following parameter values may be set in the params.txt file:

Parameters common to all algorithms:
DOMAIN - Valid values: FuncApp, TimeSeries, CaliHouse
EVALTYPE - The algorithm to use to train the network - Valid values: LEEA, FULL, SGD, RMS
	(note: FULL is a traditional generational EA)


The following parameters are only applicable to evolution-based algorithms:
THREADS - number of threads to use 
MAXGENERATIONS - maximum number of generations 
POPSIZE - size of population 
MUTATIONPOWER - maximum mutation size 
MUTATIONPOWERDECAY - amount to decay the mutation power by the end of the run
	(e.g. 0.99 = mutation power will be 1% as much at the end of the run as at the start)
MUTATIONRATE - proportion of connections to mutate in each offspring 
MUTATIONRATEDECAY - amount to decay the mutation rate by the end of the run
	(e.g. 0.99 = mutation ratewill be 1% as much at the end of the run as at the start)
SEXPROPORTION - the proportion of offspring to generate via sexual reproduction
SELECTIONPROPORTION - the proportion of individuals eligible to reproduce
	(e.g. 0.4 = only the top 40% of individuals can be selected for reproduction)
SPECIESCOUNT - number of species
TRACKFITNESSSTRIDE - the number of generations between saving fitness data
USEFITNESSBANK - boolean indicating whether fitness inheritance should be used
FITNESSBANKDECAYRATE - the decay rate for fitness inheritance

The following parameters are only applicable to LEEA:
SAMPLECOUNT - number of training examples to evaluate each generation 

The following parameters are only applicable to SGD and RMSProp
BPLEARNINGRATE - the initial learning rate
BPLEARNINGRATEDECAY - the learning rate decay rate (applied each epoch)
RMSCONNECTIONQUEUE - boolean indicating whether to use a queue-style or decay-style RMSProp
	(by default, a queue style is used as this was found to perform better)
RMSPROPK - when RMSProp is implemented via a queue, how many recent magnitudes should be stored
RMSCONNECTIONDECAY - when RMSProp is implemented with a decay, this is the decay rate
BPMAXEPOCHS - maximum number of epochs to run

ADDING A NEW DOMAIN
---------------------
To add a new domain, perform the following steps:
1) Add a class that implements the IDomain interface.  For an example of this, 
see the CaliHouseDomain class.
2) Add a class that implements the IDomainEvaluator interface.  For an example of this,
see the CaliHouseEvaluator class.
3) (optional) Modify Program.cs (Main method) so that one may set the domain via the params file.


---------------------
3. SUPPORT
---------------------

For all questions, comments, bug reports, suggestions, or friendly hellos, 
send email to GregoryMorse07@gmail.com