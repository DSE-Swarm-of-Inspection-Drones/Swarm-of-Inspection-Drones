from SALib.sample import saltelli
from SALib.analyze import sobol
import matplotlib.pyplot as plt
import numpy as np
import math

### Declaring camera and inspection surface data ###
wing_area = 122.6 * 2  # [m2]
fuselage_area = 2 * np.pi * 4.05 * 37.57  # [m2]
total_tail_area = 105  # [m2]
total_area = (wing_area + fuselage_area + total_tail_area)  # [m2]
big_area = fuselage_area * 0.6 + wing_area * 0.5 + total_tail_area
small_area = total_area - big_area
area = {"Large100":total_area, "Large60":big_area, "IRvario": big_area, "IRthermol": big_area, "IRthermolbad": big_area, "Small100":total_area, "Small40": small_area}  # Field of View [deg]

FOV = {"Large100":46.8,
       "Large60":46.8,
       "IRvario": 24.6,
       "IRthermol": 18,
       "IRthermolbad": 18,
       "Small100":12,
       "Small40": 12}  # [deg]
n_pixels = {"Large100":[8192, 5460],
            "Large60":[8192, 5460] ,
            "IRvario": [1024, 96],
            "IRthermol": [382, 288],
            "IRthermolbad": [382, 288],
            "Small100":[5120, 2880],
            "Small40": [5120, 2880]}  # number of pixels
refresh_rate = {"Large100":1.4,
                "Large60": 1.4,
                "IRvario": 240,
                "IRthermol": 80,
                "IRthermolbad": 80,
                "Small100": 120,
                "Small40": 120} # [Hz]
size_crack = {"Large100":1,
              "Large60":1,
              "IRvario": 5,
              "IRthermol": 5,
              "IRthermolbad": 5,
              "Small100":1,
              "Small40": 1}  # [mm]
shutter_speed = {"Large100":0.0005,
                 "Large60":0.0005,
                 "IRvario": 1/240,
                 "IRthermol": 1/240,
                 "IRthermolbad": 1/80,
                 "Small100":1/1000,
                 "Small40": 1/1000}  #[s], shutter speed of IR camera is uncertain and may need to be confirmed

styles = {"Large100":'dashed', "Large60":'dashed', "IRvario": 'dotted', "IRthermol": 'dotted', "IRthermolbad": 'dotted', "Small100": 'solid', "Small40": 'solid'}
markers = {"Large100":'v', "Large60":'p', "IRvario": 's', "IRthermol": '4', "IRthermolbad": '2', "Small100": '>', "Small40": '*'}
labels = {"Large100": "Zenmuse, total surface",
          "Large60":"Zenmuse, upper surface",
          "IRvario": "VarioCAM, upper surface",
          "IRthermol": "ThermolIMAGER, upper surface (E = 4 ms)",
          "IRthermolbad": "ThermolIMAGER, upper surface (E = 12.5 ms)",
          "Small100":"Hero10, total surface",
          "Small40": "Hero10, lower surface"}
inspection_list = ["Large100", "Large60", "IRvario", "IRthermol", "IRthermolbad", "Small100", "Small40"]

x = []
y = []
speed_list = []
time_list = []
### Looping over all camera options to find each inspection time vs speed graph ###
for i in range(len(inspection_list)):
    speed = np.arange(0.01, 1, 0.001)  # [m/s]
    inspection_type = inspection_list[i]
    blur_size_accepted = size_crack[inspection_type]/3  # [mm]
    blur_length = speed * shutter_speed[inspection_type] * 1000 # [mm]
    size_pixel = blur_size_accepted - blur_length # [mm]
    sidelength = size_pixel * n_pixels[inspection_type][0] # [mm]


    t_inspection = (area[inspection_type]/(sidelength/1000))/speed # [s]
    d_aircraft = (sidelength/(FOV[inspection_type]/180*np.pi))/1000 # [m]

    min_value = 1000000000

    for j in range(len(t_inspection)):
        t = t_inspection[j]
        if t < 0:
            t_inspection[j] = t_inspection[j-1]
            speed[j] = speed[j-1]
        if t > 10**8:
            t_inspection[j] = t_inspection[j-1]
            speed[j] = speed[j-1]
        if t > 0 and t < min_value:
            min_value = t

    idx = (np.where(t_inspection==min_value)[0])
    print(f'------{labels[inspection_type]}------ \nminimum time = {round(min_value,3)} seconds ({round(min_value/60,3)} minutes)   \nspeeeed for minimum time = {round(speed[idx][0],3)} meters per second\ndrone to aircraft distance = {round(d_aircraft[idx][0],3)} meters\namount of drones = {math.ceil(min_value/60/30)} drones\n\n')


    if i < 2 or i > 4:
        plt.figure(1)
        plt.loglog(speed, t_inspection, label=labels[inspection_type], linestyle=styles[inspection_type],
                   marker=markers[inspection_type], markevery=50)
    else:
        x.append(speed)
        y.append(t_inspection)
        plt.figure(2)
        plt.loglog(speed, t_inspection, label=labels[inspection_type], linestyle=styles[inspection_type],
                   marker=markers[inspection_type], markevery=50)

    speed_list.append(speed)
    time_list.append(t_inspection)


### Sensitivity Analysis of calculated time ###
problem = {
    'num_vars': 2,
    'names': ['Area', 'speed'],
    'bounds': [[400, 2000], [0.01, 10]],
}

param_values = saltelli.sample(problem, 1024)

Y = np.zeros([param_values.shape[0]])

for i, X in enumerate(param_values):
    Y[i] = X[0] / (X[1] * n_pixels[inspection_type][0] * blur_size_accepted - X[1] ** 2 * shutter_speed[
        inspection_type])

Si = sobol.analyze(problem, Y)
Si.plot()


### Plotting the inspection time graphs ###
plt.figure(1)
plt.title('Visual cameras')
plt.legend()
plt.grid()
plt.xlabel('Speed [m/s]')
plt.ylabel('Inspection time [s]')
plt.ylim(10**4/7, 10**7)
plt.xlim(0.008, 0.8)
plt.tight_layout()

plt.figure(2)
plt.fill_between(x[0], y[1], y[2], alpha=0.2, label='Inspection time margin of ThermoIMAGER')
plt.title('Thermal cameras')
plt.legend(loc='upper left')
plt.grid()
plt.xlabel('Speed [m/s]')
plt.ylabel('Inspection time [s]')
plt.ylim(10**4/7, 10**7)
plt.xlim(0.008, 0.8)
plt.tight_layout()
plt.show()
