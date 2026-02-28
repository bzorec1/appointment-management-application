package com.bzorec.hairsalonappointments

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.navigation.compose.rememberNavController
import com.bzorec.hairsalonappointments.ui.navigation.NavGraph
import com.bzorec.hairsalonappointments.ui.theme.HairSalonAppointmentsTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            HairSalonAppointmentsTheme {
                val navController = rememberNavController()
                NavGraph(navController = navController)
            }
        }
    }
}
