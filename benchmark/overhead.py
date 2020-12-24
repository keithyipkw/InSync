import sys
import numpy as np
import pandas as pd

def main():
    df = pd.read_csv(sys.argv[1], names=["Method", "Time"])
    print(df.groupby("Method").describe().to_csv())


if __name__ == "__main__":
    main()