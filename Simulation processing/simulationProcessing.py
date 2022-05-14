# %%
import os 
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.cm as cm
from scipy.stats import t #https://towardsdatascience.com/how-to-calculate-confidence-intervals-in-python-a8625a48e62b

#Increase text size
plt.rcParams.update({'font.size': 25})

#Set figure background to not be transparent
plt.rcParams['figure.facecolor'] = 'white'

#Set tight bbox inches for figure save
plt.rcParams['savefig.bbox'] = 'tight'


# %%
def listdir_fullpath(d, returnOnlyPaths = False):
    allPaths = [os.path.join(d, f) for f in os.listdir(d)]

    if returnOnlyPaths:
        allPaths = [path for path in allPaths if os.path.isdir(path)]
    else:
        #get only files ending in .txt
        allPaths = [path for path in allPaths if os.path.isfile(path) and path.endswith(".txt")]
    return allPaths

def getConfidenceInterval(percentage, data):
    n = len(data)
    m = np.mean(data)
    s = np.std(data)
    t_crit = np.abs(t.ppf((1-percentage)/2,n))
    interval = t_crit * s #/ np.sqrt(n)
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
                line = line.replace(',', '.')
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
        self.inspectionTimeBottom, self.inspectionTimeTop = getConfidenceInterval(0.9, self.inspectionTimes)

        self.inspectionTimeStd = np.std(self.inspectionTimes)
        self.inspectionTimeBottomStd = self.inspectionTimeAverage - self.inspectionTimeStd
        self.inspectionTimeTopStd = self.inspectionTimeAverage + self.inspectionTimeStd

        self.collisionNumbersBottom, self.collisionNumbersTop = getConfidenceInterval(0.9, self.collisionNumbers)
        self.collisionNumberStd = np.std(self.collisionNumbers)
        self.collisionNumberBottomStd = self.collisionNumberAverage - self.collisionNumberStd
        self.collisionNumberTopStd = self.collisionNumberAverage + self.collisionNumberStd

# %%
dataLocalPath = 'Simulation processing\\1,30'

cwd = os.getcwd()
print(os.getcwd())
dataPath = os.path.join(cwd, dataLocalPath)
my_list = listdir_fullpath(dataPath, returnOnlyPaths=True)

print(my_list)

# %%
simulationBatches = []
for i in my_list:
    simulationBatches.append(simulationBatch(i))

#sort the batches by drone number
simulationBatches.sort(key=lambda x: x.droneNumber)

#for each batch, print the average inspection time, average simulation time, and average collision number
for i in simulationBatches:
    print(i.droneNumber, i.inspectionTimeAverage, i.simulationTimeAverage, i.collisionNumberAverage)

# %%
#plot the average inspection time, average simulation time, and average collision number

fig0 = plt.figure(figsize=(20,10))
ax0 = fig0.add_subplot(111)
ax1 = ax0.twinx()

ax0.plot([i.droneNumber for i in simulationBatches], [i.inspectionTimeAverage for i in simulationBatches], 'r', label = "Inspection Time")
#fill inbetween the confidence interval
ax0.fill_between([i.droneNumber for i in simulationBatches], [i.inspectionTimeBottom for i in simulationBatches], [i.inspectionTimeTop for i in simulationBatches], alpha=0.2, color = "red", label = "95% Confidence Interval")
#plt.plot([i.droneNumber for i in simulationBatches], [i.simulationTimeAverage for i in simulationBatches], 'bo')
ax1.plot([i.droneNumber for i in simulationBatches], [i.collisionNumberAverage for i in simulationBatches], 'g', label = "Collision Number")
ax1.fill_between([i.droneNumber for i in simulationBatches], [i.collisionNumbersBottom for i in simulationBatches], [i.collisionNumbersTop for i in simulationBatches], alpha=0.2, color = "green", label = "95% Confidence Interval")

#generate legend
ax0.legend(loc='upper left')
ax1.legend(loc='upper right')
#set x title
ax0.set_xlabel("Number of drones")
#set y title
ax0.set_ylabel("Inspection Time (s)")
ax1.set_ylabel("Number of collisions")
#set title
ax0.set_title("Inspection Time and Number of Collisions vs Number of drones")

#add grid to figure
ax0.grid(True)

#save figure
fig0.savefig(os.path.join(dataPath, "inspectionTimeAndCollisionNumber.png"))

# %%
#plot the average inspection time, average simulation time, and average collision number

fig0 = plt.figure(figsize=(20,10))
ax0 = fig0.add_subplot(111)
ax1 = ax0.twinx()

ax0.plot([i.droneNumber for i in simulationBatches], [i.inspectionTimeAverage for i in simulationBatches], 'r')
#fill inbetween the confidence interval
ax0.fill_between([i.droneNumber for i in simulationBatches], [i.inspectionTimeBottomStd for i in simulationBatches], [i.inspectionTimeTopStd for i in simulationBatches], alpha=0.2, color = "red")
#plt.plot([i.droneNumber for i in simulationBatches], [i.simulationTimeAverage for i in simulationBatches], 'bo')
#ax1.plot([i.droneNumber for i in simulationBatches], [i.collisionNumberAverage for i in simulationBatches], 'g')
#ax1.fill_between([i.droneNumber for i in simulationBatches], [i.collisionNumberBottomStd for i in simulationBatches], [i.collisionNumberTopStd for i in simulationBatches], alpha=0.2, color = "green")


# %%
#make a new figure
fig1 = plt.figure(figsize=(20,10))
ax2 = fig1.add_subplot(111)

#create a list of colors for every drone number in simulationBatches
colors = []
for i in simulationBatches:
    #convert droneNumber into color
    colors.append(cm.tab20c(i.droneNumber/len(simulationBatches)))

#for each batch, plot the paretoPoints on a scatter plot with a color gradient based on the drone number
for index, i in enumerate(simulationBatches):
    ax2.scatter(i.paretoPoints[:,0], i.paretoPoints[:,1], marker='o', color = colors[index], label = str(i.droneNumber))

#add color bar using colors

#fig1.colorbar(cm.ScalarMappable(cmap="tab20c"), ax=ax2)

#generate legend
ax2.legend(loc='upper right', ncol=3, title = "Number of drones", markerscale=3)
#set x title
ax2.set_xlabel("Inspection Time (s)")
#set y title
ax2.set_ylabel("Number of collisions")
#set title
ax2.set_title("Collision number vs Inspection time")

#add grid to figure
ax2.grid()

#save figure
fig1.savefig(os.path.join(dataPath, "paretoPoints.png"))

# %%
def createGaussian(data):
    mu, sigma = np.mean(data), np.std(data)
    x = np.linspace(mu - 3*sigma, mu + 3*sigma, 100)
    y = np.exp(-(x - mu)**2 / (2 * sigma**2)) / (sigma * np.sqrt(2 * np.pi))
    return x,y


# %%
def plotDistribution(data, title, xlabel, ylabel):
    fig = plt.figure(figsize=(8,7))
    ax = fig.add_subplot(111)
    #plot the inspection time distribution for the first and last batch
    ax.hist(data, bins=10, density=True, alpha=0.5)

    #fit a gaussian to the inspection time distribution
    x, y = createGaussian(data)
    x_bot, x_top = getConfidenceInterval(0.95, data)

    #select x and y values within the confidence interval
    x_in = x[(x > x_bot) & (x < x_top)]
    y_in = y[(x > x_bot) & (x < x_top)]

    #Set title
    ax.set_title(title)
    #Set x label
    ax.set_xlabel(xlabel)
    #Set y label
    ax.set_ylabel(ylabel)

    ax.plot(x, y)
    ax.fill_between(x_in, 0, y_in, alpha=0.2, color = "red", label = "Mean 95 % Confidence Interval")

    #generate legend
    ax.legend(loc='upper left')
    #save figure
    fig.savefig(os.path.join(dataPath, title + ".png"))

# %%
plotDistribution(simulationBatches[0].inspectionTimes, "Inspection time distribution for 2 drones", "Inspection Time (s)", "Probability")
plotDistribution(simulationBatches[-1].inspectionTimes, "Inspection time distribution for 30 drones", "Inspection Time (s)", "Probability")

# %%
plotDistribution(simulationBatches[0].collisionNumbers, "Number of collisions distribution for 2 drones", "Number of collisions", "Probability")
plotDistribution(simulationBatches[-1].collisionNumbers, "Number of collisions distribution for 30 drones", "Number of collisions", "Probability")


plt.show()