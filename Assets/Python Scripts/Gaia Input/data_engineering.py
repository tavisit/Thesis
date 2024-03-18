import json

import astropy.units as u
import numpy as np
from astropy.coordinates import SkyCoord, Galactic
from astroquery.gaia import Gaia


class GaiaData:

    def __init__(self, entry_size):
        self.data = self.get_gaia_data_top_n(entry_size)
        self.sun_velocity = np.array([-11.1, 244, 7.25]) * u.km / u.s  # Sun's velocity in the Galactic frame: [U, V, W]

        coord = SkyCoord(ra=self.data['ra'].values * u.degree,
                         dec=self.data['dec'].values * u.degree,
                         distance=(1.0 / (self.data['parallax'].values * u.mas.to(u.arcsec))) * u.kpc,
                         pm_ra_cosdec=self.data['pmra'].values * u.mas / u.yr,
                         pm_dec=self.data['pmdec'].values * u.mas / u.yr,
                         radial_velocity=self.data['radial_velocity'].values * u.km / u.s,
                         frame='icrs')
        self.galactic_coord = coord.transform_to(Galactic)

    def output_data_csv(self, file_name, columns):
        output_dict = {"Items": self.data[columns].to_dict('records')}
        # Serialize the dictionary to JSON and write to file
        with open(file_name, 'w') as json_file:
            json.dump(output_dict, json_file, indent=2)

    def prepare_data_for_csv(self, name_mapping):
        self.positionComputation()
        self.velocityComputation()

        self.data["name"] = self.data['designation'].map(name_mapping).fillna(self.data['designation']).apply(
            lambda x: "Star " + x)

        # measured in kpc
        self.data['position'] = self.data.apply(lambda row: {'x': row['pos_x'], 'y': row['pos_y'], 'z': row['pos_z']},
                                                axis=1)
        # measured in kpc/Myr
        self.data['velocity'] = self.data.apply(
            lambda row: {'x': row['velocity_x'], 'y': row['velocity_y'], 'z': row['velocity_z']},
            axis=1)
        self.data["mass"] = self.estimate_mass(self.data['phot_g_mean_mag'].values, self.data['parallax'].values)
        self.data["temperature"] = self.data["teff_gspphot"]

    def get_gaia_data_top_n(self, n: int):
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

        data = result.to_pandas()

        data.columns = map(str.lower, data.columns)
        # Convert Gaia data to a Pandas DataFrame
        return data

    def estimate_mass(self, g_mag, parallax):
        distance_pc = 1000 / parallax

        # Estimate absolute magnitude in G-band
        M_G = g_mag - 5 * np.log10(distance_pc / 10)
        L_Lsun = 10 ** ((4.74 - M_G) / 2.5)  # Simplified luminosity estimation
        M_Msun = L_Lsun ** (1 / 3.5)

        return M_Msun

    def velocityComputation(self):
        gal_vel = (self.galactic_coord.velocity.d_xyz.to(u.km / u.s) - self.sun_velocity[:, None]).value
        self.data['velocity_x'] = gal_vel[0] * 1.0227
        self.data['velocity_y'] = gal_vel[1] * 1.0227
        self.data['velocity_z'] = gal_vel[2] * 1.0227

        print(self.data[['velocity_x', 'velocity_y', 'velocity_z']].describe())

    def positionComputation(self):
        self.data['pos_x'] = (self.galactic_coord.distance *
                              np.cos(self.galactic_coord.l) *
                              np.cos(self.galactic_coord.b)) \
            .to(u.kpc).value

        self.data['pos_y'] = (self.galactic_coord.distance *
                              np.sin(self.galactic_coord.l) *
                              np.cos(self.galactic_coord.b)) \
            .to(u.kpc).value

        self.data['pos_z'] = (self.galactic_coord.distance *
                              np.sin(self.galactic_coord.b)) \
            .to(u.kpc).value
