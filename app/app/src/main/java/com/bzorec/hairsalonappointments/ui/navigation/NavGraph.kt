package com.bzorec.hairsalonappointments.ui.navigation

import androidx.compose.foundation.layout.WindowInsets
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.DateRange
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.Person
import androidx.compose.material3.*
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import com.bzorec.hairsalonappointments.ui.screens.*

sealed class Screen(val route: String) {
    object Today : Screen("today")
    object AllAppointments : Screen("all_appointments")
    object Profile : Screen("profile")
    object NewAppointment : Screen("new_appointment")
    object AppointmentDetail : Screen("appointment/{id}") {
        fun createRoute(id: Int) = "appointment/$id"
    }
}

@Composable
fun NavGraph(navController: NavHostController) {
    val navBackStackEntry by navController.currentBackStackEntryAsState()
    val currentRoute = navBackStackEntry?.destination?.route
    val showBottomBar = currentRoute in setOf(Screen.Today.route, Screen.AllAppointments.route, Screen.Profile.route)

    Scaffold(
        contentWindowInsets = WindowInsets(0, 0, 0, 0),
        bottomBar = {
            if (showBottomBar) {
                NavigationBar {
                    NavigationBarItem(
                        selected = currentRoute == Screen.Today.route,
                        onClick = {
                            navController.navigate(Screen.Today.route) {
                                popUpTo(Screen.Today.route) { inclusive = false }
                                launchSingleTop = true
                            }
                        },
                        icon = { Icon(Icons.Default.Home, contentDescription = null) },
                        label = { Text("Danes") }
                    )
                    NavigationBarItem(
                        selected = currentRoute == Screen.AllAppointments.route,
                        onClick = {
                            navController.navigate(Screen.AllAppointments.route) {
                                popUpTo(Screen.Today.route) { inclusive = false }
                                launchSingleTop = true
                            }
                        },
                        icon = { Icon(Icons.Default.DateRange, contentDescription = null) },
                        label = { Text("Termini") }
                    )
                    NavigationBarItem(
                        selected = currentRoute == Screen.Profile.route,
                        onClick = {
                            navController.navigate(Screen.Profile.route) {
                                popUpTo(Screen.Today.route) { inclusive = false }
                                launchSingleTop = true
                            }
                        },
                        icon = { Icon(Icons.Default.Person, contentDescription = null) },
                        label = { Text("Profil") }
                    )
                }
            }
        }
    ) { innerPadding ->
        NavHost(
            navController = navController,
            startDestination = Screen.Today.route,
            modifier = Modifier.padding(innerPadding)
        ) {
            composable(Screen.Today.route) {
                TodayScreen(
                    onNavigateToNew = { navController.navigate(Screen.NewAppointment.route) },
                    onNavigateToDetail = { id -> navController.navigate(Screen.AppointmentDetail.createRoute(id)) }
                )
            }

            composable(Screen.AllAppointments.route) {
                AllAppointmentsScreen(
                    onNavigateToDetail = { id -> navController.navigate(Screen.AppointmentDetail.createRoute(id)) }
                )
            }

            composable(Screen.Profile.route) {
                ProfileScreen()
            }

            composable(Screen.NewAppointment.route) {
                NewAppointmentScreen(
                    onNavigateBack = { navController.popBackStack() }
                )
            }

            composable(Screen.AppointmentDetail.route) { backStackEntry ->
                val id = backStackEntry.arguments?.getString("id")?.toIntOrNull() ?: return@composable
                AppointmentDetailScreen(
                    appointmentId = id,
                    onNavigateBack = { navController.popBackStack() }
                )
            }
        }
    }
}
