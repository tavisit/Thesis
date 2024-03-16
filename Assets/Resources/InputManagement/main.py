import astropy.units as u
from astropy.coordinates import SkyCoord, Galactic
from astroquery.gaia import Gaia
import numpy as np
from astroquery.simbad import Simbad
import pandas as pd
import json

# Constants
GALACTIC_CENTER_DISTANCE = 8.34 * u.kpc  # Sun's distance from the Galactic center
SUN_VELOCITY = np.array([-11.1, 244, 7.25]) * u.km / u.s  # Sun's velocity in the Galactic frame: [U, V, W]

star_names = [
    "Sirius B", "Canopus", "Arcturus", "Proxima", "Vega", "Rigel", "Procyon", "Betelgeuse", "Achernar", "Altair",
    "Aldebaran", "Antares", "Spica", "Pollux", "Fomalhaut", "Deneb", "Regulus", "Castor", "Gacrux", "Bellatrix",
    "Elnath", "Miaplacidus", "Alnilam", "Alnair", "Alpheratz", "Regor", "Dubhe", "Mirfak", "Wezen", "Sargas",
    "Kaus Australis", "Avior", "Alkaid", "Menkalinan", "Acrux", "Almach", "Polaris", "Mirach", "Saiph", "Scheat",
    "Diphda", "Mizar", "Algorab", "Algol", "Hamal", "Electra", "Taygeta", "Maia", "Merope", "Alcyone", "Atlas",
    "Pleione", "Zeta Puppis", "Thuban", "Alderamin", "Denebola", "Gienah", "Eltanin", "Rastaban", "Alphecca",
    "Unukalhai", "Schedar", "Deneb Kaitos", "Mirzam", "Aludra", "Suhail", "Mintaka", "Gomeisa", "Enif", "Scheat",
    "Markab", "Alpheratz", "Caph", "Schedar", "Miram", "Algenib", "Algol", "Almach", "Mira", "Tsih", "Celaeno",
    "Sterope", "Merak", "Phecda", "Megrez", "Alioth", "Mizar", "Alcor", "Benetnash", "Sadr", "Errai", "Alderamin",
    "Ankaa", "Fomalhaut", "Vindemiatrix", "Shaula", "Sargas", "Kaus Media", "Nunki", "Rasalhague"
]

# Initialize Simbad
Simbad.reset_votable_fields()
Simbad.add_votable_fields('ids')

# Dictionary to hold our mappings
star_name_to_gaia_dr3 = {}

# Loop through each star name, query SIMBAD, and extract the Gaia DR3 ID
for name in star_names:
    try:
        result_table = Simbad.query_object(name)
        ids = result_table['IDS'][0].split('|')
        gaia_id = next((id for id in ids if id.startswith('Gaia DR3 ')), None)
        star_name_to_gaia_dr3[gaia_id] = name
    except Exception as e:
        print(f"Error retrieving data for {name}: {e}")


def temperature_to_rgb(temperature):
    if temperature < 1000:
        temperature = 1000
    elif temperature > 40000:
        temperature = 40000

    # Temperature to RGB conversion
    if temperature <= 6600:
        r = 255
        g = 99.4708025861 * np.log(temperature / 100) - 161.1195681661
        if temperature <= 1900:
            b = 0
        else:
            b = 138.5177312231 * np.log(temperature / 100 - 10) - 305.0447927307
    else:
        r = 329.698727446 * ((temperature / 100 - 60) ** -0.1332047592)
        g = 288.1221695283 * ((temperature / 100 - 60) ** -0.0755148492)
        b = 255

    # Ensure RGB values are within byte range
    r = min(max(0, r), 255)
    g = min(max(0, g), 255)
    b = min(max(0, b), 255)

    return int(r), int(g), int(b)


def estimate_mass(g_mag, parallax):
    distance_pc = 1000 / parallax

    # Estimate absolute magnitude in G-band
    M_G = g_mag - 5 * np.log10(distance_pc / 10)
    L_Lsun = 10 ** ((4.74 - M_G) / 2.5)  # Simplified luminosity estimation
    M_Msun = L_Lsun ** (1 / 3.5)

    return M_Msun


def get_gaia_data_top_n(n: int):
    # Ensure to select radial_velocity if you want to compute 3D velocity vector accurately
    query = f"""
                SELECT TOP {n}
                designation,
                teff_gspphot, 
                phot_g_mean_mag,
                ra, 
                dec, 
                parallax, 
                pmra, 
                pmdec, 
                radial_velocity
                
                FROM gaiadr3.gaia_source 
                
                WHERE parallax_over_error > 10 
                AND radial_velocity IS NOT NULL
                AND phot_g_mean_flux_over_error > 50
                AND phot_bp_mean_flux_over_error > 20
                AND phot_rp_mean_flux_over_error > 20
            """
    job = Gaia.launch_job(query)
    result = job.get_results()

    # Convert Gaia data to a Pandas DataFrame
    return result.to_pandas()


data = get_gaia_data_top_n(10000)
# Convert to SkyCoord object
coord = SkyCoord(ra=data['ra'].values * u.degree,
                 dec=data['dec'].values * u.degree,
                 distance=(1.0 / (data['parallax'].values * u.mas.to(u.arcsec))) * u.kpc,
                 pm_ra_cosdec=data['pmra'].values * u.mas / u.yr,
                 pm_dec=data['pmdec'].values * u.mas / u.yr,
                 radial_velocity=data['radial_velocity'].values * u.km / u.s,
                 frame='icrs')

# Convert to Galactic coordinates
galactic_coord = coord.transform_to(Galactic)
# Compute positions relative to the Galactic center
# Assuming the Sun is at (0, 0, 0) in this simplified model
data['pos_x'] = (galactic_coord.distance * np.cos(galactic_coord.l) * np.cos(galactic_coord.b)).to(u.kpc).value
data['pos_y'] = (galactic_coord.distance * np.sin(galactic_coord.l) * np.cos(galactic_coord.b)).to(u.kpc).value
data['pos_z'] = (galactic_coord.distance * np.sin(galactic_coord.b)).to(u.kpc).value

# Compute velocities in the Galactic frame
# This involves converting proper motion and radial velocity to 3D velocities, and adjusting by the Sun's motion
gal_vel = (galactic_coord.velocity.d_xyz.to(u.km / u.s) - SUN_VELOCITY[:, None]).value
data['velocity_x'] = gal_vel[0]
data['velocity_y'] = gal_vel[1]
data['velocity_z'] = gal_vel[2]
# Convert to DataFrame for better display

data[["r", "g", "b"]] = data["teff_gspphot"].apply(lambda temp: temperature_to_rgb(temp)).apply(pd.Series)

data_df = pd.DataFrame(data)

data_df["name"] = data_df['designation'] \
    .map(star_name_to_gaia_dr3) \
    .fillna(data_df['designation']) \
    .apply(lambda x: "Star " + x)
data_df["mass"] = estimate_mass(data['phot_g_mean_mag'].values, data['parallax'].values)


def normalize_color(row):
    return {'r': row['r'] / 255.0, 'g': row['g'] / 255.0, 'b': row['b'] / 255.0, 'a': 1}


data_df["color"] = data_df.apply(normalize_color, axis=1)
data_df['position'] = data_df.apply(lambda row: {'x': row['pos_x'], 'y': row['pos_y'], 'z': row['pos_z']}, axis=1)
data_df['velocity'] = data_df.apply(
    lambda row: {'x': row['velocity_x'], 'y': row['velocity_y'], 'z': row['velocity_z']}, axis=1)

# Define the necessary columns for the output DataFrame
necessary_df = data_df[['name', "mass", "color", 'position', 'velocity']]

# Convert DataFrame to a list of dictionaries
data_list = necessary_df.to_dict('records')

# Wrap the list in a dictionary under the 'Items' key
final_dict = {"Items": data_list}

# Define the output file path
output_path = "./output/galactic_data.json"

# Serialize the dictionary to JSON and write to file
with open(output_path, 'w') as json_file:
    json.dump(final_dict, json_file, indent=2)
