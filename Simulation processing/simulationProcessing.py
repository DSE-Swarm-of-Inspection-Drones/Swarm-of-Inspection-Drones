import os 
import numpy as np
import matplotlib.pyplot as plt
from scipy.stats import t #https://towardsdatascience.com/how-to-calculate-confidence-intervals-in-python-a8625a48e62b

def listdir_fullpath(d):
    return [os.path.join(d, f) for f in os.listdir(d)]

def getConfidenceInterval(percentage, data):
    n = len(data)
    m = np.mean(data)
    s = np.std(data)
    t_value = t.ppf(percentage, n-1)
    interval = t_value * s / np.sqrt(n)
    return m - interval, m + interval

class simulationBatch:
    def __init__(self, simulationFolder):
        self.droneNumber = int(simulationFolder.split("\\")[-1])
        self.getSimulations(simulationFolder)

    def getSimulations(self, simulationFolder):
        self.simulationFiles = listdir_fullpath(simulationFolder)
        self.inspectionTimes = []
        self.simulationTimes = []
        self.collisionNumbers = []
        self.paretoPoints = []
        for file in self.simulationFiles:
            #index = int(file)
            with open(file, "r") as txt_file:
                line = txt_file.readlines()[0]
            inspectionTime = float(line.split("/")[1])
            collisionNumber = int(line.split("/")[3])
            self.inspectionTimes.append(inspectionTime)
            self.simulationTimes.append(float(line.split("/")[2]))
            self.collisionNumbers.append(collisionNumber)
            self.paretoPoints.append(np.array([inspectionTime, collisionNumber]))
        
        self.inspectionTimes = np.array(self.inspectionTimes)
        self.simulationTimes = np.array(self.simulationTimes)
        self.collisionNumbers = np.array(self.collisionNumbers)
        self.paretoPoints = np.array(self.paretoPoints)

        self.inspectionTimeAverage = np.mean(self.inspectionTimes)
        self.simulationTimeAverage = np.mean(self.simulationTimes)
        self.collisionNumberAverage = np.mean(self.collisionNumbers)
        self.inspectionTimeBottom, self.inspectionTimeTop = getConfidenceInterval(0.99, self.inspectionTimes)
        self.collisionNumbersBottom, self.collisionNumbersTop = getConfidenceInterval(0.99, self.collisionNumbers)


print(os.getcwd())
my_list = listdir_fullpath('./Simulation processing/2,30')

print(my_list)

simulationBatches = []
for i in my_list:
    simulationBatches.append(simulationBatch(i))

#sort the batches by drone number
simulationBatches.sort(key=lambda x: x.droneNumber)

#for each batch, print the average inspection time, average simulation time, and average collision number
for i in simulationBatches:
    print(i.droneNumber, i.inspectionTimeAverage, i.simulationTimeAverage, i.collisionNumberAverage)

#plot the average inspection time, average simulation time, and average collision number

fig0 = plt.figure()
ax0 = fig0.add_subplot(111)
ax1 = ax0.twinx()

ax0.plot([i.droneNumber for i in simulationBatches], [i.inspectionTimeAverage for i in simulationBatches], 'r')
#fill inbetween the confidence interval
ax0.fill_between([i.droneNumber for i in simulationBatches], [i.inspectionTimeBottom for i in simulationBatches], [i.inspectionTimeTop for i in simulationBatches], alpha=0.2, color = "red")
#plt.plot([i.droneNumber for i in simulationBatches], [i.simulationTimeAverage for i in simulationBatches], 'bo')
ax1.plot([i.droneNumber for i in simulationBatches], [i.collisionNumberAverage for i in simulationBatches], 'g')
ax1.fill_between([i.droneNumber for i in simulationBatches], [i.collisionNumbersBottom for i in simulationBatches], [i.collisionNumbersTop for i in simulationBatches], alpha=0.2, color = "green")

#make a new figure
fig1 = plt.figure()
ax2 = fig1.add_subplot(111)

#for each batch, plot the paretoPoints on a scatter plot
for i in simulationBatches:
    ax2.scatter(i.paretoPoints[:,0], i.paretoPoints[:,1], marker='o')

plt.show()