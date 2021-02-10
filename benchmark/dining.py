import sys
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import matplotlib.ticker as plticker
import seaborn as sns

def main():
    sns.set_style("whitegrid")
    # plt.style.use('seaborn-whitegrid')
    pd.set_option("display.max_rows", None)

    df = pd.read_csv(sys.argv[1], names=["Method", "N", "Time"])
    des = df.groupby(["Method", "N"]).describe()
    print(des)

    fig = plt.figure(figsize=(8, 6), dpi=100)
    ax = plt.axes()
    ax.set_title("Finishing Time of Dining Philosophers")
    ax.margins(x=0)
    ax.xaxis.set_major_locator(plticker.MultipleLocator(base=1))
    ax.yaxis.set_major_locator(plticker.MultipleLocator(base=10))
    colors = {"Ordered": "#71c040", "SmartAndPolite":"#fd4d1b"}
    for method in des.index.unique(level="Method"):
        d = des.loc[method]
        x = d.index
        color = colors[method]
        ax.plot(x, d[("Time", "mean")], label=f"{method}", color=color)
        ax.fill_between(x, d[("Time", "25%")], d[("Time", "75%")], label=f"{method} Mid 50%-ile", alpha=0.2, color=color)
        ax.fill_between(x, d[("Time", "min")], d[("Time", "max")], label=f"{method} Min to Max", alpha=0.07, color=color)
        ax.set_ylabel("Seconds")
        ax.set_xlabel("Number of Philosophers")
    idealX = range(2, 33)
    idealY = [10 * x / ((x - x % 2) / 2) for x in idealX]
    ax.plot(idealX, idealY, label="Theoretical Minimum", color="#53acfc")
    ax.set_ylim(ymin=0)
    fig.tight_layout()
    plt.legend(loc="upper left")
    plt.show()

if __name__ == "__main__":
    main()