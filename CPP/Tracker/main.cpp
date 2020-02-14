/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */

/* 
 * File:   main.cpp
 * Author: Administrator
 *
 * Created on February 14, 2020, 9:26 PM
 */

#include <stdio.h>

/*
 * 
 */
int main(int argc, char **argv)
{
    for (int i = 0; i < argc; ++i)
    {
        printf("%s ",argv[i]);
    }

    printf("\n");

    return 0;
}
